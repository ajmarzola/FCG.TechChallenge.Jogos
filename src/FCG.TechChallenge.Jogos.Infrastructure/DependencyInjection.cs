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
            // 🔹 ELASTIC: bind seguro (NÃO misturar formatos de credencial)
            // ----------------------------------------------------------------------
            var elasticSection = configuration.GetSection("Elastic");

            services.Configure<ElasticOptions>(opts =>
            {
                opts.CloudId = (elasticSection["CloudId"] ?? string.Empty).Trim();
                opts.Index = (elasticSection["Index"] ?? "jogos").Trim().TrimEnd('}', '/', ' ');

                // DisablePing seguro
                opts.DisablePing = bool.TryParse(elasticSection["DisablePing"], out var dp) && dp;

                // Credenciais — escolha 1 formato:
                var apiKeyBase64 = elasticSection["ApiKeyBase64"]; // base64(id:secret)
                var apiKeyId = elasticSection["ApiKeyId"];     // id
                var apiKeySecret = elasticSection["ApiKey"];       // secret

                if (!string.IsNullOrWhiteSpace(apiKeyBase64))
                {
                    // ✔️ Formato B: Base64 — usa só ApiKeyBase64
                    opts.ApiKeyBase64 = apiKeyBase64.Trim();
                    opts.ApiKeyId = null;
                    opts.ApiKey = null;
                }
                else
                {
                    // ✔️ Formato A: ID + Secret
                    opts.ApiKeyId = apiKeyId?.Trim();
                    opts.ApiKey = apiKeySecret?.Trim();
                    opts.ApiKeyBase64 = null;
                }
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
