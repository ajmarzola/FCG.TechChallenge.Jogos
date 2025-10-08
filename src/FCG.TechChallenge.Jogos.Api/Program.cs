using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using FCG.TechChallenge.Jogos.Api.Endpoints.Jogos;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries;
using FCG.TechChallenge.Jogos.Infrastructure.Elastic;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Postgres")
         ?? builder.Configuration["ConnectionStrings:Postgres"];
if (string.IsNullOrWhiteSpace(cs))
{
    throw new InvalidOperationException("ConnectionStrings:Postgres vazio/ausente.");
}

var serviceBus = builder.Configuration.GetSection("ServiceBus")
               ?? throw new InvalidOperationException("ServiceBus não configurado.");

// ---------- DB ----------
builder.Services.AddDbContext<EventStoreDbContext>(opt =>
    opt.UseNpgsql(cs, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "public"))
       .UseSnakeCaseNamingConvention());

builder.Services.AddDbContext<ReadModelDbContext>(opt =>
    opt.UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Read", "public"))
       .UseSnakeCaseNamingConvention()); // <-- mantemos só UMA vez

// ---------- OPTIONS ----------
builder.Services.Configure<SqlOptions>(o => o.ConnectionString = cs);
builder.Services.Configure<ServiceBusOptions>(serviceBus);
builder.Services.AddElasticSearch(builder.Configuration);

// ---------- OUTBOX / EVENTSTORE ----------
builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddScoped<IEventStore, PgEventStore>();
builder.Services.AddScoped<IOutbox, PgOutbox>();

builder.Services.AddScoped<OutboxRepository>(_ => new OutboxRepository(cs));
builder.Services.AddScoped<IJogosReadRepository, JogosReadRepositoryElastic>();

// ---------- APP + INFRA ----------
FCG.TechChallenge.Jogos.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
FCG.TechChallenge.Jogos.Api.CompositionRoot.DependencyInjection.AddApplication(builder.Services);

// ---------- SWAGGER ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

// ---------- HEALTH: Elastic ----------
app.MapGet("/health/es", async (
    ElasticClientFactory factory,
    IOptions<ElasticsearchOptions> opt,
    JogoIndexer indexer) =>
{
    var es = factory.Create();
    var index = string.IsNullOrWhiteSpace(opt.Value.IndexName) ? "jogos" : opt.Value.IndexName!;

    try { await indexer.EnsureIndexAsync(CancellationToken.None); }
    catch (Exception ex) { return Results.Problem($"Falha ao garantir índice '{index}': {ex.Message}"); }

    // ✅ versão simples e estável
    var cluster = await es.Cluster.HealthAsync(CancellationToken.None);
    var idxExists = await es.Indices.ExistsAsync(index, CancellationToken.None);

    return Results.Ok(new
    {
        cluster = cluster.ClusterName,
        clusterStatus = cluster.Status.ToString(),
        nodes = cluster.NumberOfNodes,
        index,
        indexExists = idxExists.Exists
    });
})
.WithName("ElasticHealth")
.WithTags("Health");

app.MapPost("/health/es/smoke", async (
    ElasticClientFactory factory,
    IOptions<ElasticsearchOptions> opt,
    JogoIndexer indexer) =>
{
    var es = factory.Create();
    var index = string.IsNullOrWhiteSpace(opt.Value.IndexName) ? "jogos" : opt.Value.IndexName!;
    var id = Guid.NewGuid().ToString("N");

    await indexer.EnsureIndexAsync(CancellationToken.None);

    // indexa
    var indexResp = await es.IndexAsync(new EsJogoDoc
    {
        Id = id,
        Nome = "smoke-test",
        NomeSuggest = "smoke-test",
        Preco = 1.23m,
        CreatedUtc = DateTime.UtcNow
    }, CancellationToken.None);
    if (!indexResp.IsValidResponse) return Results.Problem($"Index falhou: {indexResp.DebugInformation}");

    // get
    var getResp = await es.GetAsync<EsJogoDoc>(id, CancellationToken.None);
    if (!getResp.Found) return Results.Problem("Get não encontrou o documento.");

    // delete
    var delResp = await es.DeleteAsync<EsJogoDoc>(id, CancellationToken.None);
    if (!delResp.IsValidResponse) return Results.Problem($"Delete falhou: {delResp.DebugInformation}");

    return Results.Ok(new { ok = true, index, id });
})
.WithName("ElasticSmoke")
.WithTags("Health");
//.Produces(StatusCodes.Status200OK)
//.Produces(StatusCodes.Status500InternalServerError);














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

// ---------- ELASTIC: garantir índice na subida ----------
using (var scope = app.Services.CreateScope())
{
    var indexer = scope.ServiceProvider.GetRequiredService<JogoIndexer>();
    await indexer.EnsureIndexAsync(CancellationToken.None);
}

// ---------- ADMIN (alimentar/excluir índice) ----------
var admin = app.MapGroup("/admin/search").WithTags("Admin - Search");

admin.MapPost("/rebuild", async (JogoIndexer indexer, ReadModelDbContext db, CancellationToken ct) =>
{
    await indexer.RebuildIndexAsync(db, ct);
    return Results.Ok(new { message = "Índice recriado e reindexado com sucesso." });
})
.WithName("AdminSearchRebuild");

admin.MapPost("/bulk-index", async (JogoIndexer indexer, ReadModelDbContext db, CancellationToken ct) =>
{
    var jogos = await db.Jogos.AsNoTracking().ToListAsync(ct);
    await indexer.BulkIndexAsync(jogos, ct);
    return Results.Ok(new { message = $"Bulk index finalizado. Documentos: {jogos.Count}" });
})
.WithName("AdminSearchBulkIndex");

admin.MapDelete("/delete-all", async (JogoIndexer indexer, CancellationToken ct) =>
{
    await indexer.DeleteAllAsync(ct);
    return Results.Ok(new { message = "Todos os documentos foram removidos do índice." });
})
.WithName("AdminSearchDeleteAll");

admin.MapDelete("/delete/{id:guid}", async (Guid id, JogoIndexer indexer, CancellationToken ct) =>
{
    await indexer.DeleteAsync(id, ct);
    return Results.Ok(new { message = $"Documento {id} removido do índice." });
})
.WithName("AdminSearchDeleteById");

// ---------- PÚBLICO: busca / autocomplete / recomendações ----------
app.MapGet("/jogos/search", async (
    string? q,
    int? page,
    int? pageSize,
    string? categoria,
    decimal? precoMin,
    decimal? precoMax,
    string? sort,
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

app.MapGet("/jogos/{id:guid}/recommendations", async (
    Guid id,
    int? size,
    RecommendationsQueries rec,
    CancellationToken ct) =>
{
    var k = size.GetValueOrDefault(10);
    var similar = await rec.SimilarByTextAsync(id, k, ct);

    var doc = similar.FirstOrDefault();
    IReadOnlyList<EsJogoDoc> sameCat = Array.Empty<EsJogoDoc>();
    if (doc is not null && !string.IsNullOrWhiteSpace(doc.Categoria))
    {
        sameCat = await rec.FromSameCategoryAsync(id, doc.Categoria, k, ct);
    }

    return Results.Ok(new
    {
        similarByText = similar.Select(d => new { id = d.Id, nome = d.Nome, preco = d.Preco, categoria = d.Categoria }),
        sameCategory = sameCat.Select(d => new { id = d.Id, nome = d.Nome, preco = d.Preco, categoria = d.Categoria })
    });
})
.WithName("JogosRecommendations")
.Produces(StatusCodes.Status200OK);

app.MapGet("/ping", () => Results.Text("pong"));

app.Run();
