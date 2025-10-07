namespace FCG.TechChallenge.Jogos.Application.DTOs
{
    public class JogoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? Categoria { get; set; }
        public int Version { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }
    }
}
