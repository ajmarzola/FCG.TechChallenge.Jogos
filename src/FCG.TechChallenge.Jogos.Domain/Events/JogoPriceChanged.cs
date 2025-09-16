namespace FCG.TechChallenge.Jogos.Domain.Events
{
    public sealed record JogoPriceChanged(Guid JogoId, decimal PrecoAnterior, decimal NovoPreco) : DomainEvent("JogoPriceChanged", 1);
}