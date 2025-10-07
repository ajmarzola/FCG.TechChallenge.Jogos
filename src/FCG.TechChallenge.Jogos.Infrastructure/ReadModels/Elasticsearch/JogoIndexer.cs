using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using Microsoft.Extensions.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
using Elasticsearch.Net;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class JogoIndexer
    {
        private readonly IElasticClient _es;
        private readonly string _index;

        public JogoIndexer(ElasticClientFactory factory, IOptions<ElasticOptions> opt)
        {
            _es = factory.Create();
            _index = string.IsNullOrWhiteSpace(opt.Value.Index) ? "jogos" : opt.Value.Index!;
        }

        public async Task EnsureIndexAsync(CancellationToken ct)
        {
            // 0) Ping – se isso falhar, é rede/auth
            var ping = await _es.PingAsync();
            if (!ping.IsValid)
                throw new InvalidOperationException($"Elastic ping falhou: {ping.OriginalException?.Message ?? ping.ServerError?.ToString()}\n{ping.DebugInformation}");

            // 1) Já existe?
            var exists = await _es.Indices.ExistsAsync(_index);
            if (exists.Exists) return;

            // 2) Tenta criar com seu mapping
            var create = await _es.Indices.CreateAsync(_index, c => EsMappings.ConfigureIndex(c, _index));
            if (!create.IsValid)
            {
                throw new InvalidOperationException(
                    $"Falha ao criar índice '{_index}': " +
                    (create.ServerError?.Error?.Reason ?? create.OriginalException?.Message ?? "sem detalhe")
                    + Environment.NewLine + create.DebugInformation);
            }

            // 3) Tenta fallback "mínimo" p/ isolar problema de analyzer/mapping
            var fallback = await _es.Indices.CreateAsync(_index, c => c.Map<EsJogoDoc>(m => m.AutoMap()));
            if (fallback.IsValid) return;

            // 4) Usa LOW-LEVEL pra capturar status & body exatos
            var low = await _es.LowLevel.Indices.CreateAsync<StringResponse>(
                _index,
                PostData.String("{}")  // body vazio/mínimo
            );

            var statusHi = create.ApiCall?.HttpStatusCode?.ToString() ?? "sem-status";
            var statusLo = low.HttpStatusCode?.ToString() ?? "sem-status";
            var reasonHi = create.OriginalException?.Message
                         ?? create.ServerError?.Error?.Reason
                         ?? "sem motivo (high level)";
            var bodyLo = low.Body ?? "(sem body)";

            throw new InvalidOperationException(
                $"Falha ao criar índice '{_index}'. " +
                $"HiStatus={statusHi}, HiReason={reasonHi} | " +
                $"LoStatus={statusLo}, LoBody={bodyLo}\n" +
                $"{create.DebugInformation}"
            );
        }

        public Task IndexAsync(Guid id, string nome, string? descricao, decimal preco, string? categoria, DateTime createdUtc, DateTime? updatedUtc, CancellationToken ct)
        {
            var doc = new EsJogoDoc
            {
                Id = id.ToString("N"),
                Nome = nome,
                NomeSuggest = nome,
                Descricao = descricao,
                Preco = preco,
                Categoria = categoria,
                CreatedUtc = createdUtc,
                UpdatedUtc = updatedUtc
            };

            return _es.IndexAsync(doc, i => i.Index(_index).Id(doc.Id), ct);
        }

        public Task PartialUpdatePrecoAsync(Guid id, decimal novoPreco, CancellationToken ct)
            => _es.UpdateAsync<EsJogoDoc, object>(
                id.ToString("N"),
                u => u.Index(_index).Doc(new { Preco = novoPreco }),
                ct
            );

        public Task DeleteAsync(Guid id, CancellationToken ct)
            => _es.DeleteAsync<EsJogoDoc>(id.ToString("N"), d => d.Index(_index), ct);

        public Task DeleteAllAsync(CancellationToken ct)
            => _es.DeleteByQueryAsync<EsJogoDoc>(q => q
                    .Index(_index)
                    .Query(qq => qq.MatchAll()),
                ct);

        public async Task BulkIndexAsync(IEnumerable<JogoRead> jogos, CancellationToken ct)
        {
            var docs = jogos.Select(j => new EsJogoDoc
            {
                Id = j.Id.ToString("N"),
                Nome = j.Nome,
                NomeSuggest = j.Nome,
                Descricao = j.Descricao,
                Preco = j.Preco,
                Categoria = j.Categoria,
                CreatedUtc = j.CreatedUtc,
                UpdatedUtc = j.UpdatedUtc
            });

            var response = await _es.BulkAsync(b =>
            {
                b.Index(_index);
                foreach (var d in docs)
                    b.Index<EsJogoDoc>(bi => bi.Document(d).Id(d.Id));
                return b;
            }, ct);

            if (response.Errors)
            {
                var errors = string.Join(" | ", response.ItemsWithErrors.Select(e => $"{e.Id}:{e.Error?.Reason}"));
                throw new InvalidOperationException($"BulkIndex com erros: {errors}");
            }

            await _es.Indices.RefreshAsync(_index, r => r, ct);
        }

        public async Task RebuildIndexAsync(ReadModelDbContext db, CancellationToken ct)
        {
            var exists = await _es.Indices.ExistsAsync(_index, d => d, ct);
            if (exists.Exists)
                await _es.Indices.DeleteAsync(_index, d => d, ct);

            await EnsureIndexAsync(ct);

            var all = db.Jogos.AsQueryable().ToList(); // para volumes grandes, pagine
            await BulkIndexAsync(all, ct);
        }
    }
}