using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Domain.Aggregates.Jogo
{
    public sealed partial class Jogo
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = "";
        public string Descricao { get; private set; } = "";
        public decimal Preco { get; private set; }
        public string Categoria { get; private set; } = "";
        public bool Retirado { get; private set; }
    }
}
