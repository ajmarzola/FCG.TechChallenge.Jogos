using Dapper;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Common;
using FCG.TechChallenge.Jogos.Application.DTOs;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Sql
{
    public sealed class JogosReadRepository : IJogosReadRepository
    {
        private readonly string _cs;

        // SqlOptions é o mesmo que você já injeta para o PgEventStore
        public JogosReadRepository(IOptions<SqlOptions> opt)
            => _cs = opt.Value.ConnectionString ?? throw new InvalidOperationException("ConnectionString vazia.");

        public async Task<Paged<JogoDto>> SearchAsync(string? termo, int page, int pageSize, CancellationToken ct)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            termo = string.IsNullOrWhiteSpace(termo) ? null : termo.Trim();

            // Troque "jogos_view" para o nome da sua tabela/VIEW de leitura
            const string from = "FROM jogo_view";

            var where = @"WHERE (@termo IS NULL OR
                            nome ILIKE '%' || @termo || '%' OR
                            descricao ILIKE '%' || @termo || '%' OR
                            categoria ILIKE '%' || @termo || '%')";

            var sqlCount = $"SELECT COUNT(*) {from} {where};";
            var sqlPage = $@"
            SELECT id, nome, descricao, preco, categoria
            {from}
            {where}
            ORDER BY nome
            LIMIT @take OFFSET @skip;";

            var skip = (page - 1) * pageSize;
            var take = pageSize;

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync(ct);

            var param = new { termo, take, skip };

            var total = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sqlCount, param, cancellationToken: ct));
            var rows = await conn.QueryAsync<JogoDto>(new CommandDefinition(sqlPage, param, cancellationToken: ct));

            return new Paged<JogoDto>(rows.ToList(), page, pageSize, total);
        }
    }
}