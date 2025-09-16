using FCG.TechChallenge.Jogos.Application.DTOs;
using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Queries.Jogos
{
    public sealed record SearchJogosQuery(string? Termo, string? Categoria, int Page = 1, int Size = 20) : IRequest<Paged<JogoDto>>;
}