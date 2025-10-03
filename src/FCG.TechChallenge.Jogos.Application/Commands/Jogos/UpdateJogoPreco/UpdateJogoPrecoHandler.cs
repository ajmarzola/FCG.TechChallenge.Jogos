using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo;
using FCG.TechChallenge.Jogos.Domain.Events;
using MediatR;
using System.Text.Json;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogoPreco
{
    public sealed class UpdateJogoPrecoHandler(IEventStore store, IOutbox outbox) : IRequestHandler<UpdateJogoPrecoCommand, Unit>
    {
        private readonly IEventStore _store = store;
        private readonly IOutbox _outbox = outbox;

        public async Task<Unit> Handle(UpdateJogoPrecoCommand request, CancellationToken ct)
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

            DomainEvent[] newEvents = jogo.DecideAlterarPreco(request.NovoPreco).ToArray();
            
            if (newEvents.Length == 0)
            {
                return Unit.Value; // nada a fazer
            }

            // expectedVersion = versão atual (nº de eventos)
            int expectedVersion = history.Count;
            await _store.AppendAsync(streamId, expectedVersion, newEvents, ct);

            // opcional: publicar no outbox para projeções / ES
            foreach (DomainEvent? ev in newEvents)
            {
                await _outbox.EnqueueAsync(ev.Type, JsonSerializer.Serialize(ev), ct);
            }

            return Unit.Value;
        }
    }
}