using FCG.TechChallenge.Jogos.Domain.Abstractions;

namespace FCG.TechChallenge.Jogos.Domain.Events
{
    public abstract record DomainEvent(string Type, int Version) : IDomainEvent
    {
        public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
    }
}