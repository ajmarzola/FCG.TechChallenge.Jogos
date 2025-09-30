using FCG.TechChallenge.Jogos.Domain.Abstractions;
using Npgsql;
using System.Text;

namespace FCG.TechChallenge.Jogos.Infrastructure.Outbox
{
    public sealed class OutboxStore : IOutbox
    {
        private readonly string _cs;

        public OutboxStore(string connectionString) => _cs = connectionString;

        public async Task EnqueueAsync(string type, string payload, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO public.outbox (id, type, payload, created_utc, status) VALUES (@id, @type, @payload::jsonb, now() AT TIME ZONE 'UTC', @status);";

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync(ct);

            var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("type", type);
            cmd.Parameters.AddWithValue("payload", Encoding.UTF8.GetBytes(payload));
            cmd.Parameters.AddWithValue("status", (int)OutboxStatus.Pending);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}