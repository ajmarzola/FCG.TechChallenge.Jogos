using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Common;
using FCG.TechChallenge.Jogos.Application.DTOs;
using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Queries.Jogos
{
    public sealed class SearchJogosHandler(IJogosReadRepository repo) : IRequestHandler<SearchJogosQuery, Paged<JogoDto>>
    {
        private readonly IJogosReadRepository _repo = repo; // ou DbContext/serviço de leitura

        public async Task<Paged<JogoDto>> Handle(SearchJogosQuery q, CancellationToken ct)
        {
            return await _repo.SearchAsync(q.Termo, q.Page, q.PageSize, ct);
        }
    }
}