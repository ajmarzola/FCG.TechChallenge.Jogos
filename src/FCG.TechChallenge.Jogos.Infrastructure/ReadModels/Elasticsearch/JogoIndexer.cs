using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Microsoft.Extensions.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class JogoIndexer
    {
        private readonly ElasticsearchClient _es;
        private readonly string _index;

        // Options: Uri, ApiKey (base64), IndexName
        public JogoIndexer(ElasticClientFactory factory, IOptions<ElasticsearchOptions> opt)
        {
            _es = factory.Create();
            _index = string.IsNullOrWhiteSpace(opt.Value.IndexName) ? "jogos" : opt.Value.IndexName!;
        }

        public async Task EnsureIndexAsync(CancellationToken ct)
        {
            var exists = await _es.Indices.ExistsAsync(_index, ct);
            if (exists.Exists) return;

            // ❌ antes: NEST/NULO
            // var create = await _es.Indices.CreateAsync(new CreateIndexRequest(_index), ct);

            // ✅ agora: usa a definição v9 com analyzers/mappings
            var createReq = EsIndexDefinition.Build(_index);
            var create = await _es.Indices.CreateAsync(createReq, ct);

            if (!create.IsValidResponse)
            {
                var reason = create.ElasticsearchServerError?.Error?.Reason ?? "sem detalhe";
                throw new InvalidOperationException($"Falha ao criar índice '{_index}': {reason}\n{create.DebugInformation}");
            }
        }


        public Task IndexAsync(
            Guid id, string nome, string? descricao, decimal preco, string? categoria,
            DateTime createdUtc, DateTime? updatedUtc, CancellationToken ct)
        {
            var doc = new EsJogoDoc
            {
                Id = id.ToString("N"),
                Nome = nome,
                NomeSuggest = nome,
                Descricao = descricao,
                Preco = preco,
                Categoria = categoria,
                CreatedUtc = createdUtc,
                UpdatedUtc = updatedUtc
            };

            // v9 usa DefaultIndex configurado no factory
            return _es.IndexAsync(doc, ct);
        }

        public Task PartialUpdatePrecoAsync(Guid id, decimal novoPreco, CancellationToken ct)
        {
            var req = new UpdateRequest<EsJogoDoc, object>(_index, id.ToString("N")) { Doc = new { Preco = novoPreco } };
            return _es.UpdateAsync(req, ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
            => _es.DeleteAsync<EsJogoDoc>(id.ToString("N"), ct);

        // Mais simples e robusto: dropa e recria (ao invés de DeleteByQuery)
        public async Task DeleteAllAsync(CancellationToken ct)
        {
            var del = await _es.Indices.DeleteAsync(_index, ct);
            if (!del.IsValidResponse)
                throw new InvalidOperationException($"Falha ao deletar índice '{_index}': {del.DebugInformation}");

            await EnsureIndexAsync(ct);
        }

        public async Task BulkIndexAsync(IEnumerable<JogoRead> jogos, CancellationToken ct)
        {
            var docs = jogos.Select(j => new EsJogoDoc
            {
                Id = j.Id.ToString("N"),
                Nome = j.Nome,
                NomeSuggest = j.Nome,
                Descricao = j.Descricao,
                Preco = j.Preco,
                Categoria = j.Categoria,
                CreatedUtc = j.CreatedUtc,
                UpdatedUtc = j.UpdatedUtc
            });

            // v9: use uma lista de IBulkOperation
            var ops = new List<IBulkOperation>();
            foreach (var d in docs)
                ops.Add(new BulkIndexOperation<EsJogoDoc>(d) { Id = d.Id });

            var req = new BulkRequest(_index) { Operations = ops };
            var resp = await _es.BulkAsync(req, ct);

            if (!resp.IsValidResponse || resp.Errors)
            {
                var errors = string.Join(" | ", resp.ItemsWithErrors.Select(e => $"{e.Id}:{e.Error?.Reason}"));
                throw new InvalidOperationException($"BulkIndex com erros: {errors}\n{resp.DebugInformation}");
            }

            _ = await _es.Indices.RefreshAsync(_index, ct);
        }

        public async Task RebuildIndexAsync(ReadModelDbContext db, CancellationToken ct)
        {
            var exists = await _es.Indices.ExistsAsync(_index, ct);
            if (exists.Exists)
            {
                var del = await _es.Indices.DeleteAsync(_index, ct);
                if (!del.IsValidResponse)
                    throw new InvalidOperationException($"Falha ao deletar índice '{_index}': {del.DebugInformation}");
            }

            await EnsureIndexAsync(ct);
            var all = db.Jogos.AsQueryable().ToList();
            await BulkIndexAsync(all, ct);
        }
    }
}
