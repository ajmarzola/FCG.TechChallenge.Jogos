using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class JogoIndexer(ElasticClientFactory factory, IOptions<ElasticOptions> opt)
    {
        private readonly IElasticClient _es = factory.Create();
        private readonly string _index = string.IsNullOrWhiteSpace(opt.Value.Index) ? "jogos" : opt.Value.Index;

        public Task IndexAsync(Guid id, string nome, string? descricao, decimal preco, string? categoria, CancellationToken ct) => _es.IndexAsync(new { id, nome, descricao, preco, categoria }, i => i.Id(id).Index(_index), ct);

        public Task PartialUpdatePrecoAsync(Guid id, decimal preco, CancellationToken ct) => _es.UpdateAsync<object>(id, u => u.Index(_index).Doc(new { preco }), ct);
    }
}