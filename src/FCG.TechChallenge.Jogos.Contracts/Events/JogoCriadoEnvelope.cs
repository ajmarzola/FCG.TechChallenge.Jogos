namespace FCG.TechChallenge.Jogos.Contracts.Events
{
    public sealed class JogoCriadoEnvelope
    {
        public Guid JogoId { get; init; }

        public string Nome { get; init; } = default!;

        public string? Descricao { get; init; }

        public decimal Preco { get; init; }

        public string? Categoria { get; init; }
    }
}