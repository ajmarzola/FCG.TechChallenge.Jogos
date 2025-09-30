namespace FCG.TechChallenge.Jogos.Contracts.Events
{
    public sealed class JogoPrecoAlteradoEnvelope
    {
        public Guid JogoId { get; init; }

        public decimal NovoPreco { get; init; }
    }
}