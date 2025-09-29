namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public enum OutboxStatus
    {
        Pending = 0,

        Processed = 1,

        Failed = 2
    }
}