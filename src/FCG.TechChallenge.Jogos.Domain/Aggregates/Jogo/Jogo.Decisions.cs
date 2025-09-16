using FCG.TechChallenge.Jogos.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo
{
    public sealed partial class Jogo
    {
        public IEnumerable<DomainEvent> DecideCriar(Guid id, string nome, string desc, decimal preco, string cat)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new InvalidOperationException("Nome obrigatório.");
            if (preco < 0) throw new InvalidOperationException("Preço inválido.");
            yield return new JogoCreated(id, nome, desc, preco, cat);
        }

        public IEnumerable<DomainEvent> DecideAlterarPreco(decimal novoPreco)
        {
            if (novoPreco < 0) throw new InvalidOperationException("Preço inválido.");
            if (Retirado) throw new InvalidOperationException("Jogo retirado.");
            if (novoPreco == Preco) yield break;
            yield return new JogoPriceChanged(Id, Preco, novoPreco);
        }

        public IEnumerable<DomainEvent> DecideRetirar()
        {
            if (Retirado) yield break;
            yield return new JogoRetired(Id);
        }
    }
}
