using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.TechChallenge.Jogos.Functions.Functions
{
    public class NotifyPurchaseCompleted
    {
        private readonly ILogger<NotifyPurchaseCompleted> _logger;

        public NotifyPurchaseCompleted(ILogger<NotifyPurchaseCompleted> logger)
        {
            _logger = logger;
        }

        [Function(nameof(NotifyPurchaseCompleted))]
        public async Task Run(
            [ServiceBusTrigger("mytopic", "mysubscription", Connection = "")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

             // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
