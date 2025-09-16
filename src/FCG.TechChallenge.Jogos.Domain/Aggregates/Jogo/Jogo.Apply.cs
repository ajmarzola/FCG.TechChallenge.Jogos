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
        public void Apply(DomainEvent e) => _ = e switch
        {
            JogoCreated ev => Apply(ev),
            JogoPriceChanged ev => Apply(ev),
            JogoRetired ev => Apply(ev),
            _ => this
        };

        private Jogo Apply(JogoCreated e)
        { Id = e.JogoId; Nome = e.Nome; Descricao = e.Descricao; Preco = e.Preco; Categoria = e.Categoria; Retirado = false; return this; }

        private Jogo Apply(JogoPriceChanged e)
        { Preco = e.NovoPreco; return this; }

        private Jogo Apply(JogoRetired e)
        { Retirado = true; return this; }
    }
}
