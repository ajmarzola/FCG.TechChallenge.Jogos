using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo;
using MediatR;
using System.Text.Json;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo
{
    public sealed class CreateJogoHandler(IEventStore store, IOutbox outbox) : IRequestHandler<CreateJogoCommand, Guid>
    {
        private readonly IEventStore _store = store;
        private readonly IOutbox _outbox = outbox;

        public async Task<Guid> Handle(CreateJogoCommand c, CancellationToken ct)
        {
            Guid id = Guid.NewGuid();
            Jogo agg = new Jogo();
            Domain.Events.DomainEvent[] evts = agg.DecideCriar(id, c.Nome, c.Descricao, c.Preco, c.Categoria).ToArray();

            // stream: "jogo-{id}"
            await _store.AppendAsync($"jogo-{id}", expectedVersion: 0, evts, ct);

            // opcional: mandar para outbox (ex.: para projeção assíncrona)
            await _outbox.EnqueueAsync("JogoCriado", JsonSerializer.Serialize(new { JogoId = id }), ct);

            return id;
        }
    }
}