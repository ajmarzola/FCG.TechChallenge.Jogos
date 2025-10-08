using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries;

namespace FCG.TechChallenge.Jogos.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ... (seus outros registros: DbContext, Outbox, etc.)

            services.Configure<ElasticsearchOptions>(opts =>
                configuration.GetSection("Elasticsearch").Bind(opts));

            services.AddSingleton<ElasticClientFactory>();
            services.AddSingleton<JogoIndexer>();
            services.AddSingleton<JogoSearchQueries>();      // se existirem já migradas p/ client 9
            services.AddSingleton<RecommendationsQueries>(); // idem

            return services;
        }
    }
}
