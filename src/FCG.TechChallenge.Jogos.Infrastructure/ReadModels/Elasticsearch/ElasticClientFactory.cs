using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class ElasticClientFactory(IOptions<ElasticOptions> opt)
    {
        public IElasticClient Create()
        {
            var uriString = opt.Value.Uri;

            if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException("Elastic:Uri ausente ou inválida. Defina 'Elastic:Uri' nas configs/variáveis de ambiente.");
            }

            var settings = new ConnectionSettings(uri).DefaultIndex(opt.Value.Index ?? "jogos").EnableApiVersioningHeader();

            if (!string.IsNullOrWhiteSpace(opt.Value.Username))
            {
                settings = settings.BasicAuthentication(opt.Value.Username, opt.Value.Password);
            }

            return new ElasticClient(settings);
        }
    }
}