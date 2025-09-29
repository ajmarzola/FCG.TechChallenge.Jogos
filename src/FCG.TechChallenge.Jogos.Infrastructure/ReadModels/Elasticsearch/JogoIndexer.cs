namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class JogoIndexer(ElasticClientFactory factory)
    {
        private readonly IElasticClient _es = factory.Create();

        public Task IndexAsync(Guid id, string nome, string? descricao, decimal preco, string? categoria, CancellationToken ct)
            => _es.IndexAsync(new
            {
                id,
                nome,
                descricao,
                preco,
                categoria
            }, i => i.Id(id).Index("jogos"), ct);

        public Task PartialUpdatePrecoAsync(Guid id, decimal preco, CancellationToken ct)
            => _es.UpdateAsync<object>(id, u => u.Index("jogos").Doc(new { preco }), ct);
    }
}
