using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Application.DTOs
{
    public class PagamentoResponseDto
    {
        public Guid PagamentoId { get; set; }
        public string Status { get; set; } // Ex: "Aprovado", "Recusado"
        public string Mensagem { get; set; }
    }
}
