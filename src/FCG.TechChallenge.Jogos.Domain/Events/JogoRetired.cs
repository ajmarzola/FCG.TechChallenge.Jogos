namespace FCG.TechChallenge.Jogos.Domain.Events
{
    public sealed record JogoRetired(Guid JogoId) : DomainEvent("JogoRetired", 1);
}