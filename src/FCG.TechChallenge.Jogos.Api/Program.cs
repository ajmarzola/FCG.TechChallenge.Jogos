using FCG.TechChallenge.Jogos.Api.Endpoints.Jogos;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Sql;
using FCG.TechChallenge.Jogos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var cs = builder.Configuration.GetConnectionString("Postgres") ?? builder.Configuration["ConnectionStrings:Postgres"];

if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("ConnectionStrings:Postgres vazio/ausente.");
}

var serviceBus = builder.Configuration.GetSection("ServiceBus") ?? throw new InvalidOperationException("ServiceBus não configurado.");

// ---------- DB ----------
builder.Services.AddDbContext<EventStoreDbContext>(opt => opt.UseNpgsql(cs, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "public")).UseSnakeCaseNamingConvention());

builder.Services.AddDbContext<ReadModelDbContext>(opt => opt.UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Read", "public")).UseSnakeCaseNamingConvention()); // <-- mantemos só UMA vez

// ---------- OPTIONS ----------
builder.Services.Configure<SqlOptions>(o => o.ConnectionString = cs);
builder.Services.Configure<ServiceBusOptions>(serviceBus);

// ---------- OUTBOX / EVENTSTORE ----------
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddScoped<IEventStore, PgEventStore>();
builder.Services.AddScoped<IOutbox, PgOutbox>();

builder.Services.AddScoped<OutboxRepository>(_ => new OutboxRepository(cs));
builder.Services.AddScoped<IJogosReadRepository, JogosReadRepository>();

// ---------- APP + INFRA ----------
FCG.TechChallenge.Jogos.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
FCG.TechChallenge.Jogos.Api.CompositionRoot.DependencyInjection.AddApplication(builder.Services);

// ---------- SWAGGER ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// CONFIGURAÇÃO DA INTEGRAÇÃO
// Aqui dizemos: "Sempre que alguém pedir IPagamentoIntegrationService, 
// crie um PagamentoIntegrationService e use este endereço base."
builder.Services.AddHttpClient<IPagamentoIntegrationService, PagamentoIntegrationService>(client =>
{
    // IMPORTANTE: Esta URL deve ser onde o SEU projeto de Pagamentos está rodando.
    // Se for Docker, geralmente é o nome do container: http://pagamentos-api
    // Se for rodando local no Visual Studio, pode ser algo como http://localhost:5002
    var url = builder.Configuration["PaymentServiceUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(url);

    var key = builder.Configuration["PaymentServiceApiKey"];
    if (!string.IsNullOrWhiteSpace(key))
    {
        client.DefaultRequestHeaders.Add("x-functions-key", key);
    }
});

builder.Services.AddControllers();

var app = builder.Build();

// endpoints de health
// app.MapHealthChecks("/health/live");
// app.MapHealthChecks("/health/ready");

// Middleware Prometheus (expõe /metrics)
app.UseHttpMetrics();          // métricas de request/latência
app.MapMetrics("/metrics");    // endpoint padrão Prometheus

// ---------- SWAGGER UI ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilita rotas dos controllers (ex: api/Compras/ComprarJogo)
app.MapControllers();

// ---------- ENDPOINTS EXISTENTES ----------
app.MapJogoCommands();
app.MapJogoQueries();

app.MapGet("/", () => "OK");

// ---------- HEALTH (DB) ----------
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("HealthLiveness");

app.MapGet("/health/ready", async () =>
{
    try
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1", conn);
        await cmd.ExecuteScalarAsync();
        return Results.Ok(new { status = "ready", db = "ok" });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "not ready",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
})
.WithName("HealthReadiness");

app.MapControllers();
// ---------- PÚBLICO: busca ----------
app.MapGet("/jogos/search", async (
    string? q,
    int? page,
    int? pageSize,
    IJogosReadRepository repository,
    CancellationToken ct) =>
{
    var p = page.GetValueOrDefault(1);
    var s = pageSize.GetValueOrDefault(20);
    var result = await repository.SearchAsync(
        termo: q,
        page: p,
        pageSize: s,
        ct: ct);
    return Results.Ok(result);
})
.WithName("JogosSearch")
.Produces(StatusCodes.Status200OK);

app.MapGet("/ping", () => Results.Text("pong"));

app.Run();
