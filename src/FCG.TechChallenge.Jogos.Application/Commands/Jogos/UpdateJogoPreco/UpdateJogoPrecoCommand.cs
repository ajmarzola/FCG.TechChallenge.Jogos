using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogoPreco
{
    public sealed record UpdateJogoPrecoCommand(Guid JogoId, decimal NovoPreco) : IRequest;
}