using Dapper;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Common;
using FCG.TechChallenge.Jogos.Application.DTOs;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Sql
{
    public sealed class JogosReadRepository(IOptions<SqlOptions> opt) : IJogosReadRepository
    {
        private readonly string _cs = opt.Value.ConnectionString ?? throw new InvalidOperationException("ConnectionString vazia.");

        public async Task<Paged<JogoDto>> SearchAsync(string? termo, int page, int pageSize, CancellationToken ct)
        {
            var sqlWhere = string.IsNullOrWhiteSpace(termo)
                ? ""
                : "WHERE (unaccent(nome) ILIKE unaccent(@termo) OR unaccent(categoria) ILIKE unaccent(@termo))";

            const string sqlCountTpl = @"SELECT COUNT(*) FROM public.jogo_read {WHERE}";
            const string sqlPageTpl = @"
        SELECT id, nome, descricao, preco, categoria, version, created_utc AS CreatedUtc, updated_utc AS UpdatedUtc
        FROM public.jogo_read
        {WHERE}
        ORDER BY nome
        OFFSET @skip LIMIT @take;";

            var sqlCount = sqlCountTpl.Replace("{WHERE}", sqlWhere);
            var sqlPage = sqlPageTpl.Replace("{WHERE}", sqlWhere);

            var skip = (page - 1) * pageSize;
            var take = pageSize;

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync(ct);

            var p = new { termo = $"%{termo}%", skip, take };

            var total = await conn.ExecuteScalarAsync<int>(sqlCount, p);
            var items = (await conn.QueryAsync<JogoDto>(sqlPage, p)).ToList();

            return new Paged<JogoDto>(items, total, page, pageSize);
        }


        public async Task<JogoDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = @"SELECT id, nome, descricao, preco, categoria, version, created_utc AS CreatedUtc, updated_utc AS UpdatedUtc
                         FROM public.jogo_read
                         WHERE id = @id";

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync(ct);
            return await conn.QueryFirstOrDefaultAsync<JogoDto>(sql, new { id });
        }
    }
}