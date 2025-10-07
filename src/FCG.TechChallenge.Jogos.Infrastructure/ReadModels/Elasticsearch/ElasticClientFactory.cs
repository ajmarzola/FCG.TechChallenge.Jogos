using System;
using Microsoft.Extensions.Options;
using Nest;
using Elasticsearch.Net;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class ElasticClientFactory
    {
        private readonly ElasticClient _client;

        public ElasticClientFactory(IOptions<ElasticOptions> opts)
        {
            var o = opts.Value ?? throw new ArgumentNullException(nameof(opts));

            var settings = new ConnectionSettings(new Uri(o.Uri))
                .DefaultIndex(string.IsNullOrWhiteSpace(o.Index) ? "jogos" : o.Index)
                .EnableApiVersioningHeader();

            if (!string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password))
                settings = settings.BasicAuthentication(o.Username, o.Password);

            // Para Elastic Cloud com API Key (id + key)
            if (!string.IsNullOrWhiteSpace(o.ApiKeyId) && !string.IsNullOrWhiteSpace(o.ApiKey))
                settings = settings.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(o.ApiKeyId, o.ApiKey));

            _client = new ElasticClient(settings);
        }

        public ElasticClient Create() => _client;
    }
}
