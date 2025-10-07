using Elasticsearch.Net;

using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Nest;

namespace FCG.TechChallenge.Jogos.Infrastructure.Elastic
{
    public static class ElasticDi
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<ElasticOptions>(cfg.GetSection("Elastic"));

            services.AddSingleton<IElasticClient>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;

                if (string.IsNullOrWhiteSpace(opt.CloudId) ||
                    string.IsNullOrWhiteSpace(opt.ApiKeyId) ||
                    string.IsNullOrWhiteSpace(opt.ApiKey))
                {
                    throw new InvalidOperationException("Elastic Cloud requer CloudId, ApiKeyId e ApiKey.");
                }

                var index = (opt.Index ?? "jogos").Trim().TrimEnd('}', '/', ' ');

                // Cloud-only: usa CloudId + ApiKey
                var settings = new ConnectionSettings(
                    cloudId: opt.CloudId,
                    credentials: new ApiKeyAuthenticationCredentials(opt.ApiKeyId, opt.ApiKey))
                    .DefaultIndex(index)
                    .PrettyJson()
                    .DisableDirectStreaming()   // facilita debugar (DebugInformation)
                    .ThrowExceptions();

                if (opt.DisablePing)
                {
                    settings = settings.DisablePing();
                }

                // CloudId usa CloudConnectionPool; sniffing já não é utilizado
                return new ElasticClient(settings);
            });

            // sobe o hosted service que pinga e garante o índice
            services.AddHostedService<ElasticBootService>();

            return services;
        }
    }
}
