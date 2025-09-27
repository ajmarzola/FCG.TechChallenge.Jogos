using FCG.TechChallenge.Jogos.Api.CompositionRoot;
using FCG.TechChallenge.Jogos.Api.Endpoints.Jogos;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("Postgres") ?? builder.Configuration["ConnectionStrings:Postgres"];

if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("ConnectionStrings:Postgres está vazio/ausente.");
}

builder.Services.AddDbContext<EventStoreDbContext>(opt => { opt.UseNpgsql(cs, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "public")).UseSnakeCaseNamingConvention(); });
builder.Services.AddDbContext<ReadModelDbContext>(opt => opt.UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Read", "public")).UseSnakeCaseNamingConvention());

builder.Services.Configure<SqlOptions>(o => o.ConnectionString = cs);
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
//app.UseHttpMetrics();    // Coleta automática de métricas HTTP
app.Run();