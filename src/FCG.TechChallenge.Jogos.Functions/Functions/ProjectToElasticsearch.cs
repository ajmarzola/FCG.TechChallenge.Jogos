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
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        [Function(nameof(ProjectToElasticsearch))]
        public async Task Run([ServiceBusTrigger(topicName: "jogos", subscriptionName: "project-elastic", Connection = "ServiceBus__ConnectionString")]
            ServiceBusReceivedMessage message, ServiceBusMessageActions actions, CancellationToken ct)
        {
            try
            {
                var type = message.ApplicationProperties.TryGetValue("type", out var t) ? t?.ToString() : null;
                logger.LogInformation("Processing message {MessageId} type={Type}", message.MessageId, type);

                switch (type)
                {
                    case "JogoCriado":
                        {
                            var e = JsonSerializer.Deserialize<JogoCriadoEnvelope>(message.Body, _json)!;
                            await CreateEntity(db, e, ct);
                            await indexer.IndexAsync(e.JogoId, e.Nome, e.Descricao, e.Preco, e.Categoria, ct);
                            break;
                        }
                    case "JogoPrecoAlterado":
                        {
                            var e = JsonSerializer.Deserialize<JogoPrecoAlteradoEnvelope>(message.Body, _json)!;
                            await UpdatePreco(db, e, ct);
                            await indexer.PartialUpdatePrecoAsync(e.JogoId, e.NovoPreco, ct);
                            break;
                        }
                    default:
                        logger.LogWarning("Unhandled event type: {Type}", type);
                        break;
                }

                await actions.CompleteMessageAsync(message, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing message {MessageId}", message.MessageId);
                await actions.AbandonMessageAsync(message, cancellationToken: ct);
            }
        }

        private static async Task CreateEntity(ReadModelDbContext db, JogoCriadoEnvelope e, CancellationToken ct)
        {
            var exists = await db.Jogos.FindAsync([e.JogoId], ct);

            if (exists is null)
            {
                db.Jogos.Add(new JogoRead
                {
                    Id = e.JogoId,
                    Nome = e.Nome,
                    Descricao = e.Descricao,
                    Preco = e.Preco,
                    Categoria = e.Categoria,
                    Version = 0,
                    CreatedUtc = DateTime.UtcNow
                });

                await db.SaveChangesAsync(ct);
            }
        }

        private static async Task UpdatePreco(ReadModelDbContext db, JogoPrecoAlteradoEnvelope e, CancellationToken ct)
        {
            var entity = await db.Jogos.FindAsync([e.JogoId], ct);

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