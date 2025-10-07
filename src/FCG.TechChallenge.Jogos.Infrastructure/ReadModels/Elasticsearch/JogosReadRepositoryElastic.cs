using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Common;
using FCG.TechChallenge.Jogos.Application.DTOs;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class JogosReadRepositoryElastic(JogoSearchQueries q) : IJogosReadRepository
    {
        private readonly JogoSearchQueries _q = q;

        public async Task<JogoDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var res = await _q.SearchAsync(
                termo: id.ToString(), page: 1, pageSize: 1,
                categoria: null, precoMin: null, precoMax: null, sort: null, ct: ct);
            return res.Items.FirstOrDefault();
        }

        public async Task<Paged<JogoDto>> SearchAsync(string? termo, int page, int pageSize, CancellationToken ct)
        {
            var res = await _q.SearchAsync(
                termo: termo, page: page, pageSize: pageSize,
                categoria: null, precoMin: null, precoMax: null, sort: null, ct: ct);

            // mapeia para Paged<JogoDto> (facets ficam de fora por enquanto)
            return new Paged<JogoDto>(res.Items.ToList(), res.Total, res.Page, res.PageSize);
        }
    }
}
