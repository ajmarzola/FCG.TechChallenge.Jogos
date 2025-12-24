using FCG.TechChallenge.Jogos.Application.DTOs;

using System.Net.Http.Json; // Necessário para enviar/ler JSON facilmente

namespace FCG.TechChallenge.Jogos.Infrastructure.Services;

// 1. Criamos uma interface para poder injetar isso depois
public interface IPagamentoIntegrationService
{
    Task<PagamentoResponseDto?> ProcessarPagamento(PagamentoRequestDto dadosPagamento);
}

// 2. A implementação real
public class PagamentoIntegrationService : IPagamentoIntegrationService
{
    private readonly HttpClient _httpClient;

    // O .NET vai injetar um HttpClient já configurado com o endereço da API de Pagamentos
    public PagamentoIntegrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagamentoResponseDto?> ProcessarPagamento(PagamentoRequestDto dadosPagamento)
    {
        try
        {
            // PASSO A: Faz a chamada POST para o outro microsserviço
            // "api/pagamentos" é o final da URL (Endpoint)
            var resposta = await _httpClient.PostAsJsonAsync("api/pagamentos", dadosPagamento);

            // PASSO B: Verifica se a requisição chegou lá (Código 200-299)
            if (!resposta.IsSuccessStatusCode)
            {
                // Se deu erro (ex: 404, 500), retornamos null ou lançamos erro
                // Aqui vou retornar null para simplificar
                return null;
            }

            // PASSO C: Lê o JSON que voltou e transforma no nosso objeto C#
            var resultado = await resposta.Content.ReadFromJsonAsync<PagamentoResponseDto>();
            return resultado;
        }
        catch (Exception ex)
        {
            // Se o outro servidor estiver desligado, cai aqui
            Console.WriteLine($"Erro ao conectar com pagamentos: {ex.Message}");
            return null;
        }
    }
}