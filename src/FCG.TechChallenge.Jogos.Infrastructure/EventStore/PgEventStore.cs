using Dapper;
using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Domain.Events;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace FCG.TechChallenge.Jogos.Infrastructure.EventStore
{
    public sealed class PgEventStore(IOptions<SqlOptions> opt) : IEventStore
    {
        private readonly string _cs = opt.Value.ConnectionString;

        public async Task<int> AppendAsync(string streamId, int expectedVersion, IEnumerable<object> events, CancellationToken ct)
        {
            var evts = events.Cast<DomainEvent>().ToArray();

            if (evts.Length == 0)
            {
                return expectedVersion;
            }

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                var currentVersion = await conn.ExecuteScalarAsync<int?>(
                    new CommandDefinition(
                        @"SELECT COALESCE(MAX(""Version""), 0)
                      FROM ""Events""
                      WHERE ""StreamId"" = @StreamId;",
                        new { StreamId = streamId }, tx, cancellationToken: ct)) ?? 0;

                if (currentVersion != expectedVersion)
                {
                    throw new DBConcurrencyException($"Expected version {expectedVersion} but was {currentVersion} for stream {streamId}.");
                }

                var next = expectedVersion;

                foreach (var e in evts)
                {
                    next++;

                    var payload = JsonSerializer.Serialize(e);
                    var metadata = JsonSerializer.Serialize(new { e.Type, e.Version, trace = System.Diagnostics.Activity.Current?.Id });

                    await conn.ExecuteAsync(
                        new CommandDefinition(
                            @"INSERT INTO ""Events""
                          (""StreamId"", ""Version"", ""EventId"", ""Type"", ""Data"", ""Metadata"", ""CreatedUtc"")
                          VALUES (@StreamId, @Version, @EventId, @Type, @Data, @Metadata, (NOW() AT TIME ZONE 'UTC'));",
                            new
                            {
                                StreamId = streamId,
                                Version = next,
                                EventId = Guid.NewGuid(),
                                Type = e.Type,
                                Data = payload,
                                Metadata = metadata
                            }, tx, cancellationToken: ct));
                }

                await tx.CommitAsync(ct);
                return next;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<IReadOnlyList<object>> LoadAsync(string streamId, CancellationToken ct)
        {
            await using var conn = new NpgsqlConnection(_cs);
            var rows = await conn.QueryAsync<(string Type, string Data)>(
                new CommandDefinition(
                    @"SELECT ""Type"", ""Data""
                  FROM ""Events""
                  WHERE ""StreamId"" = @StreamId
                  ORDER BY ""Version"";",
                    new { StreamId = streamId }, cancellationToken: ct));

            var list = new List<object>();

            foreach (var (type, data) in rows)
            {
                var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                object? obj = type switch
                {
                    "JogoCreated" => JsonSerializer.Deserialize<JogoCreated>(data, jsonOpts),
                    "JogoPriceChanged" => JsonSerializer.Deserialize<JogoPriceChanged>(data, jsonOpts),
                    "JogoRetired" => JsonSerializer.Deserialize<JogoRetired>(data, jsonOpts),
                    _ => null
                };

                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            return list;
        }
    }
}