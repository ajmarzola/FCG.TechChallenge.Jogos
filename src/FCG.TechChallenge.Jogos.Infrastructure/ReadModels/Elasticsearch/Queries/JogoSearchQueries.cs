using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using Microsoft.Extensions.Options;

using FCG.TechChallenge.Jogos.Application.DTOs;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries
{
    public sealed class JogoSearchQueries
    {
        private readonly ElasticsearchClient _es;
        private readonly string _index;

        public JogoSearchQueries(ElasticClientFactory factory, IOptions<ElasticsearchOptions> opt)
        {
            _es = factory.Create();
            _index = string.IsNullOrWhiteSpace(opt.Value.IndexName) ? "jogos" : opt.Value.IndexName!;
        }

        public async Task<SearchResultDto<JogoDto>> SearchAsync(
            string? termo,
            int page = 1,
            int pageSize = 20,
            string? categoria = null,
            decimal? precoMin = null,
            decimal? precoMax = null,
            string? sort = null,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var must = new List<Query>();
            var filter = new List<Query>();

            // ------ TEXTO: Nome (boost 3) + Descricao (AND) via Bool.Should ------
            if (!string.IsNullOrWhiteSpace(termo))
            {
                var should = new List<Query>();

                Query qNome = new MatchQuery
                {
                    Field = "Nome",
                    Query = termo,
                    Operator = Operator.And,
                    Boost = 3.0f
                };
                should.Add(qNome);

                Query qDesc = new MatchQuery
                {
                    Field = "Descricao",
                    Query = termo,
                    Operator = Operator.And
                };
                should.Add(qDesc);

                Query qBool = new BoolQuery
                {
                    Should = should,
                    MinimumShouldMatch = 1
                };
                must.Add(qBool);
            }

            // ------ CATEGORIA: term exato ------
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                Query qCat = new TermQuery
                {
                    Field = "Categoria",
                    Value = categoria
                };
                filter.Add(qCat);
            }

            // ------ PREÇO: faixa ------
            if (precoMin.HasValue || precoMax.HasValue)
            {
                var nr = new NumberRangeQuery { Field = "Preco" };
                if (precoMin.HasValue) nr.Gte = (double)precoMin.Value;
                if (precoMax.HasValue) nr.Lte = (double)precoMax.Value;

                Query qRange = nr; // conversão implícita para Query
                filter.Add(qRange);
            }

            // ------ ORDENAÇÃO ------
            var sortList = new List<SortOptions>();
            switch (sort)
            {
                case "preco_asc":
                    sortList.Add(new SortOptions { Field = new FieldSort { Field = "Preco", Order = SortOrder.Asc } });
                    break;

                case "preco_desc":
                    sortList.Add(new SortOptions { Field = new FieldSort { Field = "Preco", Order = SortOrder.Desc } });
                    break;

                case "recentes":
                    sortList.Add(new SortOptions { Field = new FieldSort { Field = "CreatedUtc", Order = SortOrder.Desc } });
                    break;

                default:
                    // _score desc + Nome.keyword asc
                    sortList.Add(new SortOptions { Score = new ScoreSort { Order = SortOrder.Desc } });
                    sortList.Add(new SortOptions { Field = new FieldSort { Field = "Nome.keyword", Order = SortOrder.Asc } });
                    break;
            }

            var req = new SearchRequest<EsJogoDoc>(_index)
            {
                From = (page - 1) * pageSize,
                Size = pageSize,
                Query = new BoolQuery
                {
                    Must = must,
                    Filter = filter
                },
                Sort = sortList,

                // Dica: se sua versão suportar, ative o cálculo do total:
                // TrackTotalHits = new TrackHits(true)
            };

            var res = await _es.SearchAsync<EsJogoDoc>(req, ct);
            if (!res.IsValidResponse)
                throw new InvalidOperationException($"Search inválida: {res.DebugInformation}");

            // Algumas versões não expõem 'Total' fortemente tipado; para não quebrar build,
            // usamos o Count dos hits como fallback. (Se quiser total REAL, ative TrackTotalHits e
            // use res.HitsMetadata.Total?.Value se sua versão tiver.)
            long totalLong = res.Hits != null ? res.Hits.Count : 0;
            int total = totalLong > int.MaxValue ? int.MaxValue : (int)totalLong;

            var items = res.Hits.Select(h =>
            {
                var src = h.Source!;
                return new JogoDto
                {
                    Id = Guid.Parse(src.Id),
                    Nome = src.Nome,
                    Descricao = src.Descricao,
                    Preco = src.Preco,
                    Categoria = src.Categoria,
                    CreatedUtc = src.CreatedUtc,
                    UpdatedUtc = src.UpdatedUtc,
                    Highlight = null
                };
            }).ToList();

            return new SearchResultDto<JogoDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize,
                Facets = new Dictionary<string, Dictionary<string, int>>() // adicionamos depois
            };
        }

        public async Task<IReadOnlyList<string>> AutocompleteAsync(
            string prefix,
            int size = 10,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return Array.Empty<string>();

            var req = new SearchRequest<EsJogoDoc>(_index)
            {
                Size = size,
                Query = new MatchPhrasePrefixQuery
                {
                    Field = "Nome",
                    Query = prefix
                }
            };

            var res = await _es.SearchAsync<EsJogoDoc>(req, ct);
            if (!res.IsValidResponse) return Array.Empty<string>();

            return res.Hits.Select(h => h.Source!.Nome).Distinct().ToList();
        }
    }
}
