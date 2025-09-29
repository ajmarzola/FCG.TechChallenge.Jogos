namespace FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus
{
    public sealed class ServiceBusOptions
    {
        public string? ConnectionString { get; set; }

        public string? QueueName { get; set; }
    }
}