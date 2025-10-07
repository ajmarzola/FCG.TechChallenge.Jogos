using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;

using Nest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Infrastructure.Elastic
{
    public static class EsMappings
    {
        public static CreateIndexDescriptor ConfigureIndex(CreateIndexDescriptor c, string index)
        {
            return c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(1)
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("pt_text", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "asciifolding", "portuguese_stem", "portuguese_stop")
                            )
                            .Custom("pt_ngram", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "asciifolding", "portuguese_stem", "pt_edge")
                            )
                        )
                        .TokenFilters(tf => tf
                            .EdgeNGram("pt_edge", eg => eg.MinGram(2).MaxGram(15))
                            .Stop("portuguese_stop", st => st.StopWords("_portuguese_"))
                            .Stemmer("portuguese_stem", st => st.Language("portuguese"))
                        )
                        .Normalizers(nn => nn
                            .Custom("lower_norm", no => no.Filters("lowercase", "asciifolding"))
                        )
                    )
                )
                .Map<JogoRead>(m => m
                    .AutoMap()
                    .Properties(ps => ps
                        // Guid como keyword
                        .Keyword(k => k.Name(p => p.Id).IgnoreAbove(256).Normalizer("lower_norm"))

                        // Nome com analisadores PT + ngram + keyword
                        .Text(t => t.Name(p => p.Nome)
                            .Analyzer("pt_text")
                            .SearchAnalyzer("pt_text")
                            .Fields(f => f
                                .Text(tt => tt.Name("ngram").Analyzer("pt_ngram"))
                                .Keyword(kk => kk.Name("keyword").IgnoreAbove(256).Normalizer("lower_norm"))
                            )
                        )

                        // Descrição em texto livre com PT
                        .Text(t => t.Name(p => p.Descricao)
                            .Analyzer("pt_text")
                            .SearchAnalyzer("pt_text")
                        )

                        // Categoria: filtro/agg como keyword + busca full-text opcional
                        .Keyword(k => k.Name(p => p.Categoria).IgnoreAbove(256).Normalizer("lower_norm"))
                        .Text(t => t.Name("categoria_text").Analyzer("pt_text").SearchAnalyzer("pt_text"))

                        // Preço decimal: scaled_float (2 casas decimais)
                        .Number(n => n.Name(p => p.Preco).Type(NumberType.ScaledFloat).ScalingFactor(100))

                        // Version inteiro
                        .Number(n => n.Name(p => p.Version).Type(NumberType.Integer))

                        // Datas
                        .Date(d => d.Name(p => p.CreatedUtc))
                        .Date(d => d.Name(p => p.UpdatedUtc))
                    )
                );
        }
    }
}
