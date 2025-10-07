using System;
using Microsoft.Extensions.Options;
using Nest;
using Elasticsearch.Net; // <- necessário
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

namespace FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch
{
    public sealed class ElasticClientFactory
    {
        private readonly ElasticClient _client;

        public ElasticClientFactory(IOptions<ElasticOptions> opts)
        {
            var o = opts.Value ?? throw new ArgumentNullException(nameof(opts));
            if (string.IsNullOrWhiteSpace(o.Uri))
                throw new InvalidOperationException("Elastic:Uri não configurado.");

            var settings = new ConnectionSettings(new Uri(o.Uri))
                .DefaultIndex(string.IsNullOrWhiteSpace(o.Index) ? "jogos" : o.Index)
                .EnableApiVersioningHeader();

            // 1) Basic Auth (username/senha)
            if (!string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password))
            {
                settings = settings.BasicAuthentication(o.Username, o.Password);
            }
            // 2) ApiKey separada (id + key)
            else if (!string.IsNullOrWhiteSpace(o.ApiKeyId) && !string.IsNullOrWhiteSpace(o.ApiKey))
            {
                settings = settings.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(o.ApiKeyId, o.ApiKey));
            }
            // 3) ApiKey combinada no formato "id:key" (muita gente salva assim)
            else if (!string.IsNullOrWhiteSpace(o.ApiKey) && o.ApiKey.Contains(":"))
            {
                var parts = o.ApiKey.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    settings = settings.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(parts[0], parts[1]));
            }
            else
            {
                throw new InvalidOperationException(
                    "Autenticação do Elastic não configurada. Informe (Username/Password) OU (ApiKeyId + ApiKey) OU ApiKey no formato 'id:key'.");
            }

            _client = new ElasticClient(settings);
        }

        public ElasticClient Create() => _client;
    }
}
