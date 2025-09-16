namespace FCG.TechChallenge.Jogos.Domain.Events
{
    public sealed record JogoCreated(Guid JogoId, string Nome, string Descricao, decimal Preco, string Categoria) : DomainEvent("JogoCreated", 1);
}