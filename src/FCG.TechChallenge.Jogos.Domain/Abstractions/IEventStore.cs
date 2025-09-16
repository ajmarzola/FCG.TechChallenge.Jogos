namespace FCG.TechChallenge.Jogos.Domain.Abstractions
{
    public interface IEventStore
    {
        Task<int> AppendAsync(string streamId, int expectedVersion, IEnumerable<object> events, CancellationToken ct);
        Task<IReadOnlyList<object>> LoadAsync(string streamId, CancellationToken ct);
    }
}