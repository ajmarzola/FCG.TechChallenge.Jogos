using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.DTOs;
using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Queries.Jogos
{
    public sealed class GetJogoByIdHandler(IJogosReadRepository repo) : IRequestHandler<GetJogoByIdQuery, JogoDto?>
    {
        public async Task<JogoDto?> Handle(GetJogoByIdQuery q, CancellationToken ct) => await repo.GetByIdAsync(q.Id, ct);
    }
}