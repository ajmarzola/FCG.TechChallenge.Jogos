using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Application.DTOs
{
    public class PagamentoRequestDto
    {
        public Guid PedidoId { get; set; }
        public Guid UsuarioId { get; set; }
        public decimal Valor { get; set; }

        // Dados do cartão (simulação)
        public string NumeroCartao { get; set; }
        public string NomeTitular { get; set; }
        public string Cvv { get; set; }
        public string Validade { get; set; }
    }

}
