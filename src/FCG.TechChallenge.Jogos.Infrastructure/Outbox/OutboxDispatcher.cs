using Azure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Domain.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class OutboxDispatcher(
    IOptions<ServiceBusOptions> sbOpt,
    OutboxRepository repo,
    ILogger<OutboxDispatcher> log) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connStr = sbOpt.Value.ConnectionString;
            var queue = sbOpt.Value.QueueName ?? "jogos-outbox";

            await using var client = new ServiceBusClient(connStr);
            var sender = client.CreateSender(queue);

            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await repo.PeekPendingAsync(100, stoppingToken);
                if (batch.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                foreach (var item in batch)
                {
                    var msg = new ServiceBusMessage(item.Payload)
                    {
                        ContentType = "application/json",
                        Subject = item.Type,
                        MessageId = item.Id.ToString()
                    };

                    try
                    {
                        await sender.SendMessageAsync(msg, stoppingToken);
                        await repo.MarkProcessedAsync(item.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Falha publicando OutboxItem {Id}", item.Id);
                        await repo.MarkFailedAsync(item.Id, ex.Message, stoppingToken);
                    }
                }
            }
        }
    }
}