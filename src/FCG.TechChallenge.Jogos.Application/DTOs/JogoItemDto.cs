namespace FCG.TechChallenge.Jogos.Application.DTOs
{
    public class JogoItemDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string? Categoria { get; set; }
    }
}
