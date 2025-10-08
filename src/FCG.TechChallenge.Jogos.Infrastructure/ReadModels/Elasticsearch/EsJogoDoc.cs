

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class EsJogoDoc
    {
        public string Id { get; set; } = default!;           // Guid.ToString("N")
        public string Nome { get; set; } = default!;
        public string? NomeSuggest { get; set; }              // p/ autocomplete
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? Categoria { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }
    }
    // Documento indexado no Elasticsearch
    //public sealed class EsJogoDoc
    //{
    //    [Keyword(Name = "id")]
    //    public string Id { get; set; } = default!; // usaremos Guid.ToString("N")

    //    [Text(Name = "nome", Analyzer = "portuguese", SearchAnalyzer = "portuguese")]
    //    public string Nome { get; set; } = default!;

    //    [Text(Name = "nome_suggest")]
    //    public string? NomeSuggest { get; set; } // campo p/ autocomplete (edge_ngram)

    //    [Text(Name = "descricao", Analyzer = "portuguese", SearchAnalyzer = "portuguese")]
    //    public string? Descricao { get; set; }

    //    [Number(NumberType.ScaledFloat, Name = "preco", ScalingFactor = 100)]
    //    public decimal Preco { get; set; }

    //    [Keyword(Name = "categoria")]
    //    public string? Categoria { get; set; }

    //    [Date(Name = "createdUtc")]
    //    public DateTime CreatedUtc { get; set; }

    //    [Date(Name = "updatedUtc")]
    //    public DateTime? UpdatedUtc { get; set; }
    //}

    //public static class EsMappings
    //{
    //    public static CreateIndexDescriptor ConfigureIndex(this CreateIndexDescriptor c, string indexName)
    //        => c.Index(indexName)
    //            .Settings(s => s
    //                .Analysis(a => a
    //                    .Analyzers(an => an
    //                        .Custom("portuguese", ca => ca
    //                            .Tokenizer("standard")
    //                            .Filters("lowercase", "asciifolding", "portuguese_stemmer")
    //                        )
    //                    )
    //                    .TokenFilters(tf => tf
    //                        .Snowball("portuguese_stemmer", sb => sb.Language(SnowballLanguage.Portuguese))
    //                        .EdgeNGram("edge_ngram_filter", eg => eg.MinGram(2).MaxGram(20))
    //                    )
    //                    .Tokenizers(t => t)
    //                )
    //                .Setting("index.number_of_shards", 1)
    //                .Setting("index.number_of_replicas", 1)
    //            )
    //            .Map<EsJogoDoc>(m => m
    //                .AutoMap()
    //                .Properties(p => p
    //                    .Text(t => t.Name(n => n.Nome).Analyzer("portuguese").SearchAnalyzer("portuguese"))
    //                    .Text(t => t.Name(n => n.Descricao).Analyzer("portuguese").SearchAnalyzer("portuguese"))
    //                    .Text(t => t
    //                        .Name(n => n.NomeSuggest)
    //                        .Analyzer("simple")
    //                        .Fields(ff => ff
    //                            .Text(tt => tt
    //                                .Name("autocomplete")
    //                                .Analyzer("simple")
    //                                .SearchAnalyzer("simple")
    //                            )
    //                        )
    //                    )
    //                    .Keyword(k => k.Name(n => n.Categoria))
    //                    .Number(nn => nn.Name(n => n.Preco).Type(NumberType.ScaledFloat).ScalingFactor(100))
    //                    .Date(d => d.Name(n => n.CreatedUtc))
    //                    .Date(d => d.Name(n => n.UpdatedUtc))
    //                )
    //            );
    //}
}
