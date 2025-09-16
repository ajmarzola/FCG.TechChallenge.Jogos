using FCG.TechChallenge.Jogos.Application.DTOs;
using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Queries.Jogos
{
    public sealed record GetJogoByIdQuery(Guid Id) : IRequest<JogoDto?>;
}