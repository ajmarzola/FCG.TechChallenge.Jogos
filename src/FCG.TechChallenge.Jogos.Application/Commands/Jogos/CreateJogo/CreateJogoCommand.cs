using MediatR;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo
{
    public sealed record CreateJogoCommand(string Nome, string Descricao, decimal Preco, string Categoria) : IRequest<Guid>;
}