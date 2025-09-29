namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class OutboxItem
    {
        public Guid Id { get; init; }

        public string Type { get; init; } = default!;

        public string Payload { get; init; } = default!;

        public DateTime CreatedUtc { get; init; }

        public DateTime? ProcessedUtc { get; set; }

        public string? Error { get; set; }

        public OutboxStatus Status { get; set; }
    }
}