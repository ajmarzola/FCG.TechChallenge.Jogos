using FCG.TechChallenge.Jogos.Application.Abstractions;

namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class ApplicationOutboxAdapter(OutboxStore store) : IOutbox
    {
        private readonly OutboxStore _store = store;

        public Task EnqueueAsync(string type, string payload, CancellationToken ct) => _store.EnqueueAsync(type, payload, ct);
    }
}