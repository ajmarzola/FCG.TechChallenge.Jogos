using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch.Queries;
// Se tiver interfaces de leitura:
// using FCG.TechChallenge.Jogos.Application.Abstractions;

namespace FCG.TechChallenge.Jogos.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ----------------------------------------------------------------------
            // 🔹 ELASTIC: configuração manual sem Bind()
            // ----------------------------------------------------------------------
            var elasticSection = configuration.GetSection("Elastic");
            services.Configure<ElasticOptions>(opts =>
            {
                opts.Uri = elasticSection["Uri"] ?? "";
                opts.Index = elasticSection["Index"] ?? "jogos";
                opts.Username = elasticSection["Username"];
                opts.Password = elasticSection["Password"];
            });

            // ----------------------------------------------------------------------
            // 🔹 Registra serviços do Elasticsearch
            // ----------------------------------------------------------------------
            services.AddSingleton<ElasticClientFactory>();
            services.AddSingleton<JogoIndexer>();
            services.AddSingleton<JogoSearchQueries>();
            services.AddSingleton<RecommendationsQueries>();

            // ----------------------------------------------------------------------
            // 🔹 Se quiser usar ES como repositório de leitura:
            // ----------------------------------------------------------------------
            // services.AddScoped<IJogosReadRepository, JogosReadRepositoryElastic>();

            return services;
        }
    }
}
