using Dapper;
using Npgsql;

namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class OutboxRepository
    {
        private readonly string _cs;
        public OutboxRepository(string connectionString) => _cs = connectionString;

        public async Task<IReadOnlyList<OutboxItem>> PeekPendingAsync(int maxItems, CancellationToken ct = default)
        {
            const string sql = @"SELECT id, type, payload::text AS payload, created_utc, processed_utc, error, status FROM public.outbox WHERE status = 0 ORDER BY created_utc LIMIT @take;";
            await using var conn = new NpgsqlConnection(_cs);
            var rows = await conn.QueryAsync<OutboxItem>(new CommandDefinition(sql, new { take = maxItems }, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
        {
            const string sql = @"UPDATE public.outbox SET status = 1, processed_utc = now() AT TIME ZONE 'UTC', error = NULL WHERE id = @id;";
            await using var conn = new NpgsqlConnection(_cs);
            await conn.ExecuteAsync(new CommandDefinition(sql, new { id }, cancellationToken: ct));
        }

        public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
        {
            const string sql = @"UPDATE public.outbox SET status = 2, error = @error WHERE id = @id;";
            await using var conn = new NpgsqlConnection(_cs);
            await conn.ExecuteAsync(new CommandDefinition(sql, new { id, error }, cancellationToken: ct));
        }
    }
}