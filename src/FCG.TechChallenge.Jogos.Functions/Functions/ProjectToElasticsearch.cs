using Azure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Contracts.Events;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FCG.TechChallenge.Jogos.Functions.Functions
{
    public class ProjectToElasticsearch(
    ReadModelDbContext db,
    JogoIndexer indexer,
    ILogger<ProjectToElasticsearch> logger)
    {
        [Function(nameof(ProjectToElasticsearch))]
        public async Task Run(
            [ServiceBusTrigger("%ServiceBus:QueueName%", Connection = "ServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message,
            ServiceBusMessageActions actions,
            CancellationToken ct)
        {
            try
            {
                var type = message.Subject; // "JogoCriado" etc
                var json = message.Body.ToString();
                switch (type)
                {
                    case "JogoCriado":
                        var created = JsonSerializer.Deserialize<JogoCriadoEnvelope>(json)!;
                        await UpsertJogoRead(db, created, ct);
                        await indexer.IndexAsync(created.JogoId, created.Nome, created.Descricao, created.Preco, created.Categoria, ct);
                        break;

                    case "JogoPriceChanged":
                        var price = JsonSerializer.Deserialize<JogoPrecoAlteradoEnvelope>(json)!;
                        await UpdatePreco(db, price, ct);
                        await indexer.PartialUpdatePrecoAsync(price.JogoId, price.NovoPreco, ct);
                        break;

                    // outros tipos...

                    default:
                        logger.LogWarning("Tipo de evento desconhecido: {Type}", type);
                        break;
                }

                await actions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro projetando mensagem {Id}", message.MessageId);
                await actions.AbandonMessageAsync(message);
            }
        }

        private static async Task UpsertJogoRead(ReadModelDbContext db, JogoCriadoEnvelope e, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var entity = await db.Jogos.FindAsync(new object?[] { e.JogoId }, ct);
            if (entity is null)
            {
                db.Jogos.Add(new JogoRead
                {
                    Id = e.JogoId,
                    Nome = e.Nome,
                    Descricao = e.Descricao,
                    Preco = e.Preco,
                    Categoria = e.Categoria,
                    Version = 1,
                    CreatedUtc = now,
                    UpdatedUtc = now
                });
            }
            else
            {
                entity.Nome = e.Nome;
                entity.Descricao = e.Descricao;
                entity.Preco = e.Preco;
                entity.Categoria = e.Categoria;
                entity.Version++;
                entity.UpdatedUtc = now;
            }
            await db.SaveChangesAsync(ct);
        }

        private static async Task UpdatePreco(ReadModelDbContext db, JogoPrecoAlteradoEnvelope e, CancellationToken ct)
        {
            var entity = await db.Jogos.FindAsync(new object?[] { e.JogoId }, ct);
            if (entity == null)
            {
                return;
            }

            entity.Preco = e.NovoPreco;
            entity.Version++;
            entity.UpdatedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }
}
