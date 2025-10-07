using FCG.TechChallenge.Jogos.Domain.Events;

namespace FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo
{
    public sealed partial class Jogo
    {
        public IEnumerable<DomainEvent> DecideCriar(Guid id, string nome, string desc, decimal preco, string cat)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new InvalidOperationException("Nome obrigatório.");
            }

            yield return preco < 0 ? throw new InvalidOperationException("Preço inválido.") : (DomainEvent)new JogoCreated(id, nome, desc, preco, cat);
        }

        public IEnumerable<DomainEvent> DecideAlterar(Guid id, string nome, string desc, decimal preco, string cat)
        {
            if (string.IsNullOrWhiteSpace(id.ToString()) || id == Guid.Empty)
            {
                throw new InvalidOperationException("ID inválido.");
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new InvalidOperationException("Nome obrigatório.");
            }

            if (preco < 0)
            {
                throw new InvalidOperationException("Preço inválido.");
            }

            if (Excluido)
            {
                throw new InvalidOperationException("Jogo retirado.");
            }

            if ((nome == Nome) && (preco == Preco) && (desc == Descricao) && (cat == Categoria))
            {
                yield break;
            }

            yield return new JogoChanged(Id, Nome, nome, Descricao, desc, Preco, preco, Categoria, cat);
        }

        public IEnumerable<DomainEvent> DecideAlterarPreco(decimal novoPreco)
        {
            if (novoPreco < 0)
            {
                throw new InvalidOperationException("Preço inválido.");
            }

            if (Excluido)
            {
                throw new InvalidOperationException("Jogo retirado.");
            }

            if (novoPreco == Preco)
            {
                yield break;
            }

            yield return new JogoPriceChanged(Id, Preco, novoPreco);
        }

        public IEnumerable<DomainEvent> DecideDeletar()
        {
            if (Excluido)
            {
                yield break;
            }

            yield return new JogoRetired(Id);
        }
    }
}
