using FCG.TechChallenge.Jogos.Api.CompositionRoot;
using FCG.TechChallenge.Jogos.Api.Endpoints.Jogos;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Postgres") ?? builder.Configuration["ConnectionStrings:Postgres"];

builder.Services.AddDbContext<EventStoreDbContext>(opt =>
{
    opt.UseNpgsql(cs, npg =>
    {
        // Se quiser, configure compatibilidade, retry, etc.
        npg.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});

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



app.Run();

