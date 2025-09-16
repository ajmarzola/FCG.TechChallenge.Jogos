using FCG.TechChallenge.Jogos.Application.DTOs;
using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Queries.Users
{
    public sealed record GetRecommendationsQuery(Guid UserId) : IRequest<IReadOnlyList<JogoDto>>;
}