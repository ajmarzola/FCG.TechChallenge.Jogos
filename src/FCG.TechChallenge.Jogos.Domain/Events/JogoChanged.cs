namespace FCG.TechChallenge.Jogos.Domain.Events
{
    public sealed record JogoChanged(Guid JogoId, string NomeAnterior, string NovoNome, string DescricaoAnterior, string NovaDescricao, decimal PrecoAnterior, decimal NovoPreco, string CategoriaAnterior, string NovaCategoria) : DomainEvent("JogoChanged", 1);
}