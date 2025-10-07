using FCG.TechChallenge.Jogos.Api.CompositionRoot;
using FCG.TechChallenge.Jogos.Api.Endpoints.Jogos;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries;
using Microsoft.EntityFrameworkCore;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---------- CONFIG & DB ----------
var cs = builder.Configuration.GetConnectionString("Postgres") ?? builder.Configuration["ConnectionStrings:Postgres"];
if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("ConnectionStrings:Postgres está vazio/ausente.");
}

builder.Services.AddDbContext<EventStoreDbContext>(opt =>
{
    opt.UseNpgsql(cs, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "public"))
       .UseSnakeCaseNamingConvention();
});

builder.Services.AddDbContext<ReadModelDbContext>(opt =>
{
    opt.UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Read", "public"))
       .UseSnakeCaseNamingConvention();
});

builder.Services.Configure<SqlOptions>(o => o.ConnectionString = cs);
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection("ServiceBus"));


// ---------- OUTBOX / EVENTSTORE ----------
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddScoped<IEventStore, PgEventStore>();
builder.Services.AddScoped<IOutbox, PgOutbox>();

builder.Services.AddScoped<OutboxRepository>(_ => new OutboxRepository(cs));

builder.Services.AddScoped<IJogosReadRepository, JogosReadRepositoryElastic>();

// ---------- APP + INFRA (inclui Elasticsearch) ----------
FCG.TechChallenge.Jogos.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
FCG.TechChallenge.Jogos.Api.CompositionRoot.DependencyInjection.AddApplication(builder.Services);


// ---------- SWAGGER ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// ---------- SWAGGER UI ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ---------- ENDPOINTS EXISTENTES ----------
app.MapJogoCommands();
app.MapJogoQueries();

// ---------- HEALTH ENDPOINTS ----------
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

// ======================================================================
//  A PARTIR DAQUI: ELASTICSEARCH — GARANTIR ÍNDICE + ENDPOINTS
// ======================================================================

// Garante que o índice ES existe quando a API iniciar
using (var scope = app.Services.CreateScope())
{
    var indexer = scope.ServiceProvider.GetRequiredService<JogoIndexer>();
    await indexer.EnsureIndexAsync(CancellationToken.None);
}

// ------------------ ADMIN (alimentar/excluir) ------------------
// (Proteja com gateway/JWT se desejar: role ADMIN)
// Grupo: /admin/search
var admin = app.MapGroup("/admin/search")
               .WithTags("Admin - Search");

// Recria índice e reindexa TUDO (drop + create + bulk)
admin.MapPost("/rebuild", async (JogoIndexer indexer, ReadModelDbContext db, CancellationToken ct) =>
{
    await indexer.RebuildIndexAsync(db, ct);
    return Results.Ok(new { message = "Índice recriado e reindexado com sucesso." });
})
.WithName("AdminSearchRebuild");

// Indexa em massa SEM dropar (bulk a partir do read model)
admin.MapPost("/bulk-index", async (JogoIndexer indexer, ReadModelDbContext db, CancellationToken ct) =>
{
    var jogos = await db.Jogos.AsNoTracking().ToListAsync(ct);
    await indexer.BulkIndexAsync(jogos, ct);
    return Results.Ok(new { message = $"Bulk index finalizado. Documentos: {jogos.Count}" });
})
.WithName("AdminSearchBulkIndex");

// Remove TODOS os documentos (mantém o índice)
admin.MapDelete("/delete-all", async (JogoIndexer indexer, CancellationToken ct) =>
{
    await indexer.DeleteAllAsync(ct);
    return Results.Ok(new { message = "Todos os documentos foram removidos do índice." });
})
.WithName("AdminSearchDeleteAll");

// Remove 1 documento por ID
admin.MapDelete("/delete/{id:guid}", async (Guid id, JogoIndexer indexer, CancellationToken ct) =>
{
    await indexer.DeleteAsync(id, ct);
    return Results.Ok(new { message = $"Documento {id} removido do índice." });
})
.WithName("AdminSearchDeleteById");

// ------------------ PÚBLICO: BUSCA/AUTOCOMPLETE/RECOMENDAÇÕES ------------------
// /search: q, page, pageSize, categoria, precoMin, precoMax, sort
app.MapGet("/jogos/search", async (
    string? q,
    int? page,
    int? pageSize,
    string? categoria,
    decimal? precoMin,
    decimal? precoMax,
    string? sort, // "preco_asc" | "preco_desc" | "recentes" | null
    JogoSearchQueries queries,
    CancellationToken ct) =>
{
    var p = page.GetValueOrDefault(1);
    var s = pageSize.GetValueOrDefault(20);
    var result = await queries.SearchAsync(
        termo: q,
        page: p,
        pageSize: s,
        categoria: categoria,
        precoMin: precoMin,
        precoMax: precoMax,
        sort: sort,
        ct: ct);

    return Results.Ok(result);
})
.WithName("JogosSearch")
.Produces(StatusCodes.Status200OK);

// /autocomplete: prefix, size
app.MapGet("/jogos/autocomplete", async (
    string prefix,
    int? size,
    JogoSearchQueries queries,
    CancellationToken ct) =>
{
    var s = size.GetValueOrDefault(10);
    var list = await queries.AutocompleteAsync(prefix, s, ct);
    return Results.Ok(list);
})
.WithName("JogosAutocomplete")
.Produces(StatusCodes.Status200OK);

// /{id}/recommendations: similaridade + mesma categoria
app.MapGet("/jogos/{id:guid}/recommendations", async (
    Guid id,
    int? size,
    RecommendationsQueries rec,
    CancellationToken ct) =>
{
    var k = size.GetValueOrDefault(10);
    var similar = await rec.SimilarByTextAsync(id, k, ct);

    // também tentamos "mesma categoria" — simples: pega a primeira doc se existir
    var doc = similar.FirstOrDefault();
    IReadOnlyList<EsJogoDoc> sameCat = Array.Empty<EsJogoDoc>();
    if (doc is not null && !string.IsNullOrWhiteSpace(doc.Categoria))
        sameCat = await rec.FromSameCategoryAsync(id, doc.Categoria, k, ct);

    return Results.Ok(new
    {
        similarByText = similar.Select(d => new {
            id = d.Id,
            nome = d.Nome,
            preco = d.Preco,
            categoria = d.Categoria
        }),
        sameCategory = sameCat.Select(d => new {
            id = d.Id,
            nome = d.Nome,
            preco = d.Preco,   // <- corrigido
            categoria = d.Categoria
        })

    });
})
.WithName("JogosRecommendations")
.Produces(StatusCodes.Status200OK);

app.Run();
