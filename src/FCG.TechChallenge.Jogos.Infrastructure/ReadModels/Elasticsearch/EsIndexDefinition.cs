using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public static class EsIndexDefinition
    {
        // IMPORTANTE: mantenha os nomes em PascalCase se o seu factory usa DefaultFieldNameInferrer(p => p)
        public static CreateIndexRequest Build(string index)
        {
            return new CreateIndexRequest(index)
            {
                Settings = new IndexSettings
                {
                    NumberOfShards = 1,
                    NumberOfReplicas = 1
                    // Sem Analysis aqui para evitar erros de tipos/namespace.
                },
                Mappings = new TypeMapping
                {
                    Properties = new Properties
                    {
                        { "Id",         new KeywordProperty { IgnoreAbove = 256 } },
                        { "Nome",       new TextProperty() },
                        { "NomeSuggest",new TextProperty() },
                        { "Descricao",  new TextProperty() },
                        // scaled_float (duas casas) = ScalingFactor 100
                        { "Preco",      new ScaledFloatNumberProperty { ScalingFactor = 100 } },
                        { "Categoria",  new KeywordProperty { IgnoreAbove = 256 } },
                        { "CreatedUtc", new DateProperty() },
                        { "UpdatedUtc", new DateProperty() }
                    }
                }
            };
        }
    }
}
