using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogo
{
    public sealed record UpdateJogoCommand(Guid JogoId, string Nome, string Descricao, decimal Preco, string Categoria) : IRequest<Guid>;
}