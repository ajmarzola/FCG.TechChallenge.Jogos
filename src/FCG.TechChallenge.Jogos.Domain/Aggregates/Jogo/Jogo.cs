namespace FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo
{
    public sealed partial class Jogo
    {
        public Guid Id { get; private set; }

        public string Nome { get; private set; } = string.Empty;
        
        public string Descricao { get; private set; } = string.Empty;
        
        public decimal Preco { get; private set; }
        
        public string Categoria { get; private set; } = string.Empty;
        
        public bool Excluido { get; private set; }
    }
}