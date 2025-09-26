using Dapper;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FCG.TechChallenge.Jogos.Infrastructure.EventStore
{
    public sealed class PgOutbox(IOptions<SqlOptions> opt) : IOutbox
    {
        private readonly string _cs = opt.Value.ConnectionString;

        public async Task EnqueueAsync(string type, string payload, CancellationToken ct)
        {
            await using var conn = new NpgsqlConnection(_cs);

            await conn.ExecuteAsync(
                new CommandDefinition(
                    @"INSERT INTO ""OutboxMessages""
                  (""Id"", ""Type"", ""Payload"", ""CreatedUtc"")
                  VALUES (@Id, @Type, @Payload, (NOW() AT TIME ZONE 'UTC'));",
                    new { Id = Guid.NewGuid(), Type = type, Payload = payload }, cancellationToken: ct));
        }
    }
}