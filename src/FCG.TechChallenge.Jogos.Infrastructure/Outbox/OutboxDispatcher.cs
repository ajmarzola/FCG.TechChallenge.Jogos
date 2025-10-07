using Azure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection; // <-- IMPORTANTE
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;

namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class OutboxDispatcher(
        IOptions<ServiceBusOptions> sbOpt,
        IServiceProvider serviceProvider,               // <-- injete o provider, não o repo
        ILogger<OutboxDispatcher> log) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connStr = sbOpt.Value.ConnectionString;
            var queue = sbOpt.Value.QueueName ?? "jogos-outbox";

            await using var client = new ServiceBusClient(connStr);
            await using var sender = client.CreateSender(queue);

            // loop principal
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // cria um escopo por iteração (para DbContext/Repo scoped)
                    using var scope = serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<OutboxRepository>();

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
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown normal
                }
                catch (Exception ex)
                {
                    // falha inesperada: loga e espera um pouco antes de tentar novamente
                    log.LogError(ex, "Erro no loop do OutboxDispatcher");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
