using Azure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Domain.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class OutboxDispatcher(IOptions<ServiceBusOptions> sbOpt, IOutbox outbox, ILogger<OutboxDispatcher> log) : BackgroundService
    {
        private readonly string _conn = sbOpt.Value.ConnectionString!;
        private readonly string _queue = sbOpt.Value.QueueName ?? "jogos-outbox";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var client = new ServiceBusClient(_conn);
            ServiceBusSender sender = client.CreateSender(_queue);

            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await outbox.PeekPendingAsync(100, stoppingToken);
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
                        await outbox.MarkProcessedAsync(item.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Falha publicando OutboxItem {Id}", item.Id);
                        await outbox.MarkFailedAsync(item.Id, ex.Message, stoppingToken);
                    }
                }
            }
        }
    }
}