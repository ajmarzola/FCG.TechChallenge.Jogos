using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nest;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries
{
    public sealed class RecommendationsQueries(ElasticClientFactory factory, IOptions<ElasticOptions> opt)
    {
        private readonly IElasticClient _es = factory.Create();
        private readonly string _index = string.IsNullOrWhiteSpace(opt.Value.Index) ? "jogos" : opt.Value.Index!;

        // Similaridade por conteúdo (nome/descrição) sem usar MinimumTermFrequency/MinimumDocumentFrequency
        public async Task<IReadOnlyList<EsJogoDoc>> SimilarByTextAsync(Guid jogoId, int size = 10, CancellationToken ct = default)
        {
            var id = jogoId.ToString("N");

            var res = await _es.SearchAsync<EsJogoDoc>(s => s
                .Index(_index)
                .Query(q => q.MoreLikeThis(mlt => mlt
                    .Like(l => l.Document(d => d.Id(id)))
                    .Fields(f => f.Fields(ff => ff.Nome, ff => ff.Descricao))
                    .MaxQueryTerms(25) // mantém a consulta “enxuta”
                ))
                .Size(size)
                .Source(true),
                ct
            );

            return res.Documents.ToList();
        }

        // Mesma categoria (exclui o próprio)
        public async Task<IReadOnlyList<EsJogoDoc>> FromSameCategoryAsync(Guid jogoId, string? categoria, int size = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(categoria))
            {
                return Array.Empty<EsJogoDoc>();
            }

            var res = await _es.SearchAsync<EsJogoDoc>(s => s
                .Index(_index)
                .Query(q => q.Bool(b => b
                    .Must(m => m.Term(t => t.Field(f => f.Categoria).Value(categoria)))
                    .MustNot(mn => mn.Term(t => t.Field(f => f.Id).Value(jogoId.ToString("N"))))
                ))
                .Sort(ss => ss.Descending(f => f.CreatedUtc))
                .Size(size),
                ct
            );

            return res.Documents.ToList();
        }
    }
}
