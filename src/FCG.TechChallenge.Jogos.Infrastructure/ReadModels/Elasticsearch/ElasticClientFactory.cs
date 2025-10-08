using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class ElasticClientFactory
    {
        private readonly ElasticsearchClient _client;

        public ElasticClientFactory(IOptions<ElasticsearchOptions> opt)
        {
            var o = opt.Value;

            var settings = new ElasticsearchClientSettings(new Uri(o.Uri))
                .DefaultIndex(string.IsNullOrWhiteSpace(o.IndexName) ? "jogos" : o.IndexName)
                .DefaultFieldNameInferrer(p => p) // mantém PascalCase dos campos
                .EnableDebugMode();

            if (!string.IsNullOrWhiteSpace(o.ApiKey))
                settings = settings.Authentication(new Base64ApiKey(o.ApiKey)); // API key do Elastic Cloud (base64)

            _client = new ElasticsearchClient(settings);
        }

        public ElasticsearchClient Create() => _client;
    }
}
