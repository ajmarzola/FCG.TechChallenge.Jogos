namespace FCG.TechChallenge.Jogos.Domain.Abstractions
{
    public interface IOutbox
    {
        Task EnqueueAsync(string type, string payload, CancellationToken ct);
    }
}