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
            var o = opts?.Value ?? throw new ArgumentNullException(nameof(opts));

            if (string.IsNullOrWhiteSpace(o.CloudId))
                throw new InvalidOperationException("Elastic:CloudId não configurado.");
            if (string.IsNullOrWhiteSpace(o.ApiKeyId) || string.IsNullOrWhiteSpace(o.ApiKey))
                throw new InvalidOperationException("Elastic:ApiKeyId e Elastic:ApiKey são obrigatórios para Elastic Cloud.");

            var index = (o.Index ?? "jogos").Trim().TrimEnd('}', '/', ' ');

            // CloudId + ApiKey (id/secret)
            var settings = new ConnectionSettings(
                    cloudId: o.CloudId,
                    credentials: new ApiKeyAuthenticationCredentials(o.ApiKeyId, o.ApiKey))
                .DefaultIndex(index)
                .EnableApiVersioningHeader() // compat futura
                .ThrowExceptions();

            if (o.DisablePing)
                settings = settings.DisablePing();

#if DEBUG
            settings = settings
                .DisableDirectStreaming() // mostra request/response em erros
                                          // NÃO usar PrettyJson() aqui pra não introduzir '?pretty=true'
                .OnRequestCompleted(call =>
                {
                    Console.WriteLine("=== ELASTIC CALL ===");
                    Console.WriteLine(call.DebugInformation);
                });
#endif

            _client = new ElasticClient(settings);
        }

        public ElasticClient Create() => _client;
    }
}
