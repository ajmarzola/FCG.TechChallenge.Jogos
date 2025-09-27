namespace FCG.TechChallenge.Jogos.Application.Abstractions
{
    public interface IOutbox
    {
        Task EnqueueAsync(string type, string payload, CancellationToken ct);
    }
}
