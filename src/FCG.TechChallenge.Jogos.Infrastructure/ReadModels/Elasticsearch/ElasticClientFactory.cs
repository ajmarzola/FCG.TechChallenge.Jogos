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
            if (string.IsNullOrWhiteSpace(o.Uri))
                throw new InvalidOperationException("Elastic:Uri não configurado.");

            // RECOMENDO: colocar :9243 no appsettings (Elastic Cloud)
            // Ex.: https://....es....elastic-cloud.com:9243
            var settings = new ConnectionSettings(new Uri(o.Uri))
                .DefaultIndex(string.IsNullOrWhiteSpace(o.Index) ? "jogos" : o.Index)
                .EnableApiVersioningHeader() // compat com ES 9 (wire v8)
                .DisablePing()               // evita HEAD /
                //.DisableSniffing()           // Cloud não usa sniff
                .SniffOnStartup(false)
                .SniffOnConnectionFault(false);

            // Autenticação: UM dos modos abaixo
            if (!string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password))
            {
                settings = settings.BasicAuthentication(o.Username, o.Password);
            }
            else if (!string.IsNullOrWhiteSpace(o.ApiKeyId) && !string.IsNullOrWhiteSpace(o.ApiKey))
            {
                settings = settings.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(o.ApiKeyId, o.ApiKey));
            }
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

#if DEBUG
            settings = settings
                .DisableDirectStreaming() // loga corpo de erro
                                          // NÃO use PrettyJson() aqui para evitar '?pretty=true'
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
