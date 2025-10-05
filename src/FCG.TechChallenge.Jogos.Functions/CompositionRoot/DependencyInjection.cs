using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.TechChallenge.Jogos.Functions.CompositionRoot
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ElasticOptions>(configuration.GetSection("Elastic"));
            services.AddSingleton<ElasticClientFactory>();
            services.AddSingleton<JogoIndexer>();
            return services;
        }
    }
}