using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries
{
    public sealed class RecommendationsQueries
    {
        private readonly ElasticsearchClient _es;
        private readonly string _index;

        public RecommendationsQueries(ElasticClientFactory factory, IOptions<ElasticsearchOptions> opt)
        {
            _es = factory.Create();
            _index = string.IsNullOrWhiteSpace(opt.Value.IndexName) ? "jogos" : opt.Value.IndexName!;
        }

        public async Task<IReadOnlyList<EsJogoDoc>> SimilarByTextAsync(Guid jogoId, int size = 10, CancellationToken ct = default)
        {
            var id = jogoId.ToString("N");

            // pega o doc semente
            var get = await _es.GetAsync<EsJogoDoc>(new GetRequest(_index, id), ct);
            if (!get.Found || get.Source is null)
            {
                return Array.Empty<EsJogoDoc>();
            }

            var seed = string.Join(' ', new[] { get.Source.Nome, get.Source.Descricao }
                                        .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(seed))
            {
                return await FromSameCategoryAsync(jogoId, get.Source.Categoria, size, ct);
            }

            // v9: use os tipos concretos; nada de new Query(...) nem new Fields(...)
            var req = new SearchRequest<EsJogoDoc>(_index)
            {
                Size = size,
                Query = new BoolQuery
                {
                    Must = new List<Query>
                    {
                        new MultiMatchQuery
                        {
                            Query  = seed,
                            // converte string -> Field -> Fields implicitamente
                            Fields = new Field[] { "Nome", "Descricao" },
                            Type   = TextQueryType.BestFields
                        }
                    },
                    MustNot = new List<Query>
                    {
                        new TermQuery { Field = "Id", Value = id }
                    }
                }
            };

            var res = await _es.SearchAsync<EsJogoDoc>(req, ct);
            return res.IsValidResponse ? (res.Documents?.ToList() ?? new List<EsJogoDoc>()) : Array.Empty<EsJogoDoc>();
        }

        public async Task<IReadOnlyList<EsJogoDoc>> FromSameCategoryAsync(Guid jogoId, string? categoria, int size = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(categoria))
            {
                return Array.Empty<EsJogoDoc>();
            }

            var id = jogoId.ToString("N");

            var req = new SearchRequest<EsJogoDoc>(_index)
            {
                Size = size,
                Query = new BoolQuery
                {
                    Must = new List<Query>
                    {
                        new TermQuery { Field = "Categoria", Value = categoria }
                    },
                    MustNot = new List<Query>
                    {
                        new TermQuery { Field = "Id", Value = id }
                    }
                },
                // v9: SortOptions sem construtor; setar a propriedade Field/Score/etc.
                Sort = new List<SortOptions>
                {
                    new SortOptions
                    {
                        Field = new FieldSort
                        {
                            Field = "CreatedUtc",
                            Order = SortOrder.Desc
                        }
                    }
                }
            };

            var res = await _es.SearchAsync<EsJogoDoc>(req, ct);
            return res.IsValidResponse ? (res.Documents?.ToList() ?? new List<EsJogoDoc>()) : Array.Empty<EsJogoDoc>();
        }
    }
}
