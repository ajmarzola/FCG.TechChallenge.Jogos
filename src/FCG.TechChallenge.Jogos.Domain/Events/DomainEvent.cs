using FCG.TechChallenge.Jogos.Domain.Abstractions;

namespace FCG.TechChallenge.Jogos.Domain.Events
{
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
    }
}