namespace FCG.TechChallenge.Jogos.Application.Common
{
    public sealed record Paged<T>(IReadOnlyList<T> Items, int Page, int Size, int Total);
}