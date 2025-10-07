using FCG.TechChallenge.Jogos.Application.DTOs;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries
{
    public sealed class JogoSearchQueries
    {
        private readonly IElasticClient _es;
        private readonly string _index;

        public JogoSearchQueries(ElasticClientFactory factory, IOptions<Config.Options.ElasticOptions> opt)
        {
            _es = factory.Create();
            _index = string.IsNullOrWhiteSpace(opt.Value.Index) ? "jogos" : opt.Value.Index!;
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

            var must = new List<Func<QueryContainerDescriptor<EsJogoDoc>, QueryContainer>>();
            var filter = new List<Func<QueryContainerDescriptor<EsJogoDoc>, QueryContainer>>();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                must.Add(m => m.MultiMatch(mm => mm
                    .Query(termo)
                    .Fields(f => f
                        .Field(ff => ff.Nome, boost: 3.0)
                        .Field(ff => ff.Descricao))
                    .Type(TextQueryType.BestFields)
                    .Operator(Operator.And)));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
                filter.Add(f => f.Term(t => t.Field(ff => ff.Categoria).Value(categoria)));

            if (precoMin.HasValue || precoMax.HasValue)
                filter.Add(f => f.Range(r => r
                    .Field(ff => ff.Preco)
                    .GreaterThanOrEquals(precoMin.HasValue ? (double?)precoMin.Value : null) // cast p/ double?
                    .LessThanOrEquals(precoMax.HasValue ? (double?)precoMax.Value : null)
                ));

            var from = (page - 1) * pageSize;

            var res = await _es.SearchAsync<EsJogoDoc>(s => {
                s = s.Index(_index)
                     .Query(q => q.Bool(b => b.Must(must).Filter(filter)))
                     .Highlight(h => h
                        .Fields(hf => hf
                            .Field(f => f.Nome).PreTags("<mark>").PostTags("</mark>"))
                        .Fields(hf => hf
                            .Field(f => f.Descricao).PreTags("<mark>").PostTags("</mark>")
                        )
                     )
                     .Aggregations(a => a
                        .Terms("by_categoria", t => t.Field(f => f.Categoria).Size(20))
                        .Range("by_preco", r => r
                            .Field(f => f.Preco)
                            .Ranges(
                                rr => rr.To(5000),              // <= 50,00
                                rr => rr.From(5000).To(20000),  // 50–200
                                rr => rr.From(20000)            // > 200
                            )
                        )
                     )
                     .From(from)
                     .Size(pageSize);

                // Ordenação
                s = s.Sort(ss =>
                {
                    return sort switch
                    {
                        "preco_asc" => ss.Ascending(f => f.Preco),
                        "preco_desc" => ss.Descending(f => f.Preco),
                        "recentes" => ss.Descending(f => f.CreatedUtc),
                        _ => ss.Descending(SortSpecialField.Score).Ascending(f => f.Nome.Suffix("keyword"))
                    };
                });

                return s;
            }, ct);

            if (!res.IsValid) throw new InvalidOperationException($"Search inválida: {res.ServerError}");

            var items = res.Hits.Select(h => {
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
                    Highlight = new HighlightDto
                    {
                        Nome = h.Highlight?.GetValueOrDefault("nome")?.FirstOrDefault(),
                        Descricao = h.Highlight?.GetValueOrDefault("descricao")?.FirstOrDefault()
                    }
                };
            }).ToList();

            var facets = new Dictionary<string, Dictionary<string, int>>();

            // categoria
            var catAgg = res.Aggregations.Terms("by_categoria");
            facets["categoria"] = catAgg?.Buckets?
                .ToDictionary(b => b.Key as string ?? string.Empty, b => (int)b.DocCount.GetValueOrDefault())
                ?? new Dictionary<string, int>();

            // preco
            var priceAgg = res.Aggregations.Range("by_preco");
            facets["preco"] = new Dictionary<string, int>
            {
                { "<=50",  (int)(priceAgg?.Buckets.FirstOrDefault(b => b.Key == "*-5000.0")?.DocCount ?? 0) },
                { "50-200",(int)(priceAgg?.Buckets.FirstOrDefault(b => b.Key == "5000.0-20000.0")?.DocCount ?? 0) },
                { ">200",  (int)(priceAgg?.Buckets.FirstOrDefault(b => b.Key == "20000.0-*")?.DocCount ?? 0) }
            };

            return new SearchResultDto<JogoDto>
            {
                Items = items,
                Total = (int)res.Total,
                Page = page,
                PageSize = pageSize,
                Facets = facets
            };
        }

        public async Task<IReadOnlyList<string>> AutocompleteAsync(string prefix, int size = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return Array.Empty<string>();

            var res = await _es.SearchAsync<EsJogoDoc>(s => s
                .Index(_index)
                .Query(q => q.MatchPhrasePrefix(m => m.Field(f => f.Nome).Query(prefix)))
                .Size(size)
                .Source(sf => sf.Includes(i => i.Field(f => f.Nome)))
            , ct);

            return res.Hits.Select(h => h.Source!.Nome).Distinct().ToList();
        }
    }
}
