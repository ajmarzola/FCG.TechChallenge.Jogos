using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class ElasticClientFactory(IOptions<ElasticOptions> opt)
    {
        public IElasticClient Create()
        {
            var settings = new ConnectionSettings(new Uri(opt.Value.Uri))
                .DefaultIndex(opt.Value.Index ?? "jogos")
                .EnableApiVersioningHeader();

            if (!string.IsNullOrWhiteSpace(opt.Value.Username))
                settings = settings.BasicAuthentication(opt.Value.Username, opt.Value.Password);

            return new ElasticClient(settings);
        }
    }
}
