using FCG.TechChallenge.Jogos.Application.Abstractions;
using MediatR;
using System.Text.Json;
using FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo;

namespace FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo
{
    public sealed class CreateJogoHandler : IRequestHandler<CreateJogoCommand, Guid>
    {
        private readonly IEventStore _store;
        private readonly IOutbox _outbox;

        public CreateJogoHandler(IEventStore store, IOutbox outbox)
        { _store = store; _outbox = outbox; }

        public async Task<Guid> Handle(CreateJogoCommand c, CancellationToken ct)
        {
            var id = Guid.NewGuid();
            var agg = new Jogo();
            var evts = agg.DecideCriar(id, c.Nome, c.Descricao, c.Preco, c.Categoria).ToArray();

            // stream: "jogo-{id}"
            await _store.AppendAsync($"jogo-{id}", expectedVersion: 0, evts, ct);

            // opcional: mandar para outbox (ex.: para projeção assíncrona)
            await _outbox.EnqueueAsync("JogoCriado", JsonSerializer.Serialize(new { JogoId = id }), ct);

            return id;
        }
    }
}