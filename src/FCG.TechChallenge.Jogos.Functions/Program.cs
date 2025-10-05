using FCG.TechChallenge.Jogos.Functions.CompositionRoot;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights (ok)
builder.Services.AddApplicationInsightsTelemetryWorkerService().ConfigureFunctionsApplicationInsights();

// Postgres (Read Model)
var cs = builder.Configuration["Postgres"];

builder.Services.AddDbContext<ReadModelDbContext>(opt => opt.UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Read", "public")).UseSnakeCaseNamingConvention());

// Elastic
builder.Services.Configure<ElasticOptions>(builder.Configuration.GetSection("Elastic"));
builder.Services.AddSingleton<ElasticClientFactory>();
builder.Services.AddSingleton<JogoIndexer>();

// (Se quiser padronizar via CompositionRoot)
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

builder.Build().Run();
