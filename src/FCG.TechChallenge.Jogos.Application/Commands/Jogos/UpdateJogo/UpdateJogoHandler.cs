using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo;
using FCG.TechChallenge.Jogos.Domain.Events;
using MediatR;
using System.Text.Json;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogo
{
    public sealed class UpdateJogoHandler(IEventStore store, IOutbox outbox) : IRequestHandler<UpdateJogoCommand, Guid>
    {
        private readonly IEventStore _store = store;
        private readonly IOutbox _outbox = outbox;

        public async Task<Guid> Handle(UpdateJogoCommand request, CancellationToken ct)
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

            DomainEvent[] newEvents = jogo.DecideAlterar(request.JogoId, request.Nome, request.Descricao, request.Preco, request.Categoria).ToArray();

            if (newEvents.Length == 0)
            {
                return request.JogoId; // nada a fazer
            }

            // expectedVersion = versão atual (nº de eventos)
            int expectedVersion = history.Count;
            await _store.AppendAsync(streamId, expectedVersion, newEvents, ct);

            // opcional: publicar no outbox para projeções / ES
            foreach (DomainEvent? ev in newEvents)
            {
                await _outbox.EnqueueAsync(ev.Type, JsonSerializer.Serialize(ev), ct);
            }

            return request.JogoId;
        }
    }
}