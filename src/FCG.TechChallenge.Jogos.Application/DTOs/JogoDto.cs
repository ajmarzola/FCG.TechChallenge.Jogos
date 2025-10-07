namespace FCG.TechChallenge.Jogos.Application.DTOs
{
    public sealed class JogoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = default!;
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? Categoria { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }

        public HighlightDto? Highlight { get; set; }
    }

    public sealed class HighlightDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
    }
}
