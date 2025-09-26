using FCG.TechChallenge.Jogos.Application.Common;
using FCG.TechChallenge.Jogos.Application.DTOs;
using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Queries.Jogos
{
    public sealed record SearchJogosQuery(string? Termo, int Page = 1, int PageSize = 20) : IRequest<Paged<JogoDto>>;
}