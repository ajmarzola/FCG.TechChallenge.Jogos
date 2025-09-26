using FCG.TechChallenge.Jogos.Application.Common;
using FCG.TechChallenge.Jogos.Application.DTOs;

namespace FCG.TechChallenge.Jogos.Application.Abstractions
{
    public interface IJogosReadRepository
    {
        Task<Paged<JogoDto>> SearchAsync(string? termo, int page, int pageSize, CancellationToken ct);
    }
}