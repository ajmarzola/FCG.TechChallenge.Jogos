using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddApplicationInsightsTelemetryWorkerService().ConfigureFunctionsApplicationInsights();
builder.Services.AddDbContext<ReadModelDbContext>(opt => opt.UseNpgsql(builder.Configuration["Postgres"]).UseSnakeCaseNamingConvention());
builder.Build().Run();
