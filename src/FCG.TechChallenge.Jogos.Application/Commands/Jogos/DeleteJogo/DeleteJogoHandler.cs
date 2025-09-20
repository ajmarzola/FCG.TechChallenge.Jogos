using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo;
using FCG.TechChallenge.Jogos.Domain.Events;
using MediatR;
using System.Text.Json;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.DeleteJogo
{
    public sealed class DeleteJogoHandler(IEventStore store, IOutbox outbox) : IRequestHandler<DeleteJogoCommand>
    {
        private readonly IEventStore _store = store;
        private readonly IOutbox _outbox = outbox;

        public async Task Handle(DeleteJogoCommand request, CancellationToken ct)
        {
            string streamId = $"jogo-{request.JogoId}";
            IReadOnlyList<object> history = await _store.LoadAsync(streamId, ct);
            
            if (history.Count == 0)
            {
                throw new KeyNotFoundException("Jogo não encontrado.");
            }

            Jogo jogo = new Jogo();
            
            foreach (DomainEvent e in history.OfType<DomainEvent>())
            {
                jogo.Apply(e);
            }

            DomainEvent[] newEvents = jogo.DecideDeletar().ToArray();
            
            if (newEvents.Length == 0)
            {
                return; // já retirado
            }

            int expectedVersion = history.Count;
            await _store.AppendAsync(streamId, expectedVersion, newEvents, ct);

            foreach (DomainEvent? ev in newEvents)
            {
                await _outbox.EnqueueAsync(ev.Type, JsonSerializer.Serialize(ev), ct);
            }
        }
    }
}