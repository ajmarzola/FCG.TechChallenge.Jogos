using FCG.TechChallenge.Jogos.Application.DTOs;

using FCG.TechChallenge.Jogos.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FCG.TechChallenge.Jogos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    // Injetamos nosso serviço de integração
    private readonly IPagamentoIntegrationService _pagamentoService;

    public ComprasController(IPagamentoIntegrationService pagamentoService)
    {
        _pagamentoService = pagamentoService;
    }

    [HttpPost] // POST: api/compras
    [Route("ComprarJogo")]
    public async Task<IActionResult> ComprarJogo([FromBody] PagamentoRequestDto pedido)
    {
        // 1. Opcional: Aqui você validaria se o jogo existe no seu banco de dados local

        // 2. Chama o microsserviço de Pagamentos
        var resultado = await _pagamentoService.ProcessarPagamento(pedido);

        // 3. Verifica se falhou a comunicação
        if (resultado == null)
        {
            return StatusCode(503, "O sistema de pagamentos está indisponível no momento.");
        }

        // 4. Verifica se o pagamento foi RECUSADO (ex: cartão sem saldo)
        if (resultado.Status == "Recusado")
        {
            return BadRequest(new { mensagem = "Pagamento recusado pela operadora.", detalhes = resultado });
        }

        // 5. SUCESSO!
        // Aqui você liberaria o jogo na biblioteca do usuário

        return Ok(new { mensagem = "Compra realizada com sucesso!", dadosPagamento = resultado });
    }
}