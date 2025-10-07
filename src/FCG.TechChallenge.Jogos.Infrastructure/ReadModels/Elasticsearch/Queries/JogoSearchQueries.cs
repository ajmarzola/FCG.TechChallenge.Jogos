using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries
{
    public sealed class JogoSearchQueries(IElasticClient es, IOptions<ElasticOptions> opt)
    {
        private readonly string _index = string.IsNullOrWhiteSpace(opt.Value.Index) ? "jogos" : opt.Value.Index;

        public async Task<(IReadOnlyList<dynamic> items, long total)> SearchAsync(string? termo, int page, int pageSize, CancellationToken ct)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            var from = (page - 1) * pageSize;
            var must = new List<QueryContainer>();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                must.Add(new QueryContainerDescriptor<dynamic>().MultiMatch(mm => mm
                    .Query(termo)
                    .Fields("nome^3,descricao,categoria")
                    .Fuzziness(Fuzziness.Auto)));
            }

            var resp = await es.SearchAsync<dynamic>(s => s
                .Index(_index)
                .From(from)
                .Size(pageSize)
                .Query(q => must.Count == 0 ? q.MatchAll() : q.Bool(b => b.Must(must.ToArray())))
                .Sort(ss => ss.Ascending("nome")), ct);

            return !resp.IsValid ? throw new InvalidOperationException(resp.ServerError?.ToString() ?? "Falha na busca ES") : ((IReadOnlyList<dynamic> items, long total))(resp.Documents, resp.Total);
        }
    }
}