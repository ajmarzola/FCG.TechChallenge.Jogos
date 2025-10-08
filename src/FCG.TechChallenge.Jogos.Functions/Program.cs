using FCG.TechChallenge.Jogos.Functions.CompositionRoot;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
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

// (Se quiser padronizar via CompositionRoot)
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

builder.Build().Run();
