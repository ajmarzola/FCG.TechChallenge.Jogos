namespace FCG.TechChallenge.Jogos.Application.DTOs
{
    public sealed class SearchResultDto<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }

        // Facets simples: categoria e faixas de preço
        public Dictionary<string, Dictionary<string, int>> Facets { get; init; } = new();
    }
}
