namespace FCG.TechChallenge.Jogos.Domain.Abstractions
{
    public interface IOutbox
    {
        Task EnqueueAsync(string type, string payload, CancellationToken ct = default);

        Task<IReadOnlyList<OutboxItem>> PeekPendingAsync(int maxItems, CancellationToken ct = default);

        Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

        Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);
    }
}