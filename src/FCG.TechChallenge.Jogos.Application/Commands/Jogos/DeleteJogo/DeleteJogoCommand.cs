using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.DeleteJogo
{
    public sealed record DeleteJogoCommand(Guid JogoId) : IRequest<Unit>;
}