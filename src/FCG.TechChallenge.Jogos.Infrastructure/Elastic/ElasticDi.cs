using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries;
using FCG.TechChallenge.Jogos.Infrastructure.Elastic; // para o BootService (abaixo)

namespace FCG.TechChallenge.Jogos.Infrastructure.Elastic
{
    public static class ElasticDi
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration cfg)
        {
            // Lê "Elasticsearch" (v9)
            services.Configure<ElasticsearchOptions>(cfg.GetSection("Elasticsearch"));

            // Cliente e helpers
            services.AddSingleton<ElasticClientFactory>();
            services.AddSingleton<JogoIndexer>();
            services.AddSingleton<JogoSearchQueries>();
            services.AddSingleton<RecommendationsQueries>();

            // Garante índice na subida
            services.AddHostedService<ElasticBootService>();

            return services;
        }
    }
}
