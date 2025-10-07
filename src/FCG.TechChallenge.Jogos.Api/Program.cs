using FCG.TechChallenge.Jogos.Api.CompositionRoot;
using FCG.TechChallenge.Jogos.Api.Endpoints.Jogos;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using Microsoft.EntityFrameworkCore;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("Postgres") ?? builder.Configuration["ConnectionStrings:Postgres"];
var serviceBus = builder.Configuration.GetSection("ServiceBus") ?? throw new InvalidOperationException("ServiceBus:ConnectionString não configurado.");

if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("ConnectionStrings:Postgres vazio/ausente.");
}

builder.Services.AddDbContext<EventStoreDbContext>(opt => opt.UseNpgsql(cs, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "public")).UseSnakeCaseNamingConvention());
builder.Services.AddDbContext<ReadModelDbContext>(opt => opt.UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Read", "public")).UseSnakeCaseNamingConvention());

builder.Services.Configure<SqlOptions>(o => o.ConnectionString = cs);


builder.Services.Configure<ServiceBusOptions>(serviceBus);
builder.Services.Configure<ElasticOptions>(builder.Configuration.GetSection("Elastic"));

builder.Services.AddHostedService<OutboxDispatcher>();

builder.Services.AddScoped<IEventStore, PgEventStore>();
builder.Services.AddScoped<IOutbox, PgOutbox>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
WebApplication app = builder.Build();
app.MapJogoCommands();
app.MapJogoQueries();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseMetricServer();   // Endpoint - metrics
//app.UseHttpMetrics();    // Coleta autom�tica de m�tricas HTTP

// ---------- HEALTH ENDPOINTS ----------

// Liveness — rápido, sem dependências externas
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("HealthLiveness");

// Readiness — verifica conexão ao Postgres
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
        // 503 para sinalizar que ainda não está pronto
        return Results.Problem(
            title: "not ready",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
})
.WithName("HealthReadiness");



app.Run();