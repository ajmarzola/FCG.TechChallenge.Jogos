using Elasticsearch.Net;

using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

using Microsoft.Extensions.Options;

using Nest;

using System.Text;

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

            var index = (o.Index ?? "jogos").Trim().TrimEnd('}', '/', ' ');

            // --------- Normalização de credenciais ---------
            // Suportar:
            //  A) ApiKeyBase64 = base64("id:secret")
            //  B) ApiKeyId + ApiKey (secret)
            string? base64 = string.IsNullOrWhiteSpace(o.ApiKeyBase64) ? null : o.ApiKeyBase64!.Trim();

            if (base64 is not null)
            {
                // valida se decodifica em "id:secret"
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                    if (!decoded.Contains(":"))
                        throw new FormatException("ApiKeyBase64 não está no formato base64(\"id:secret\").");
                }
                catch (FormatException)
                {
                    throw new InvalidOperationException("Elastic:ApiKeyBase64 inválida. Use o valor 'Base64 encoded' exibido no painel do Elastic Cloud.");
                }
            }
            else
            {
                // monta base64 a partir de id + secret
                if (string.IsNullOrWhiteSpace(o.ApiKeyId) || string.IsNullOrWhiteSpace(o.ApiKey))
                    throw new InvalidOperationException("Configure Elastic ApiKeyBase64 OU ApiKeyId + ApiKey (secret).");

                var raw = $"{o.ApiKeyId.Trim()}:{o.ApiKey.Trim()}";
                base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
            }

            var creds = new ApiKeyAuthenticationCredentials(base64);

            var settings = new ConnectionSettings(
                    cloudId: o.CloudId.Trim(),
                    credentials: creds)
                .DefaultIndex(index)
                .EnableApiVersioningHeader()
                .ThrowExceptions()
#if DEBUG
                .DisableDirectStreaming()
                .OnRequestCompleted(call =>
                {
                    try
                    {
                        var method = call?.HttpMethod.ToString() ?? "?";
                        var url = call?.Uri?.ToString() ?? "?";
                        var code = call?.HttpStatusCode?.ToString() ?? "?";
                        Console.WriteLine($"[ELK] {method} {url} -> {code}");

                        if (call?.HttpStatusCode == 401)
                        {
                            Console.WriteLine("[ELK] 401 recebido. Verifique formato da ApiKey (Base64 'id:secret' ou id+secret) e CloudId.");
                            Console.WriteLine(call?.DebugInformation);
                        }
                    }
                    catch { /* evita falha de log em DEBUG */ }
                })
#endif
                ;

            if (o.DisablePing) settings = settings.DisablePing();

            _client = new ElasticClient(settings);
        }

        public ElasticClient Create() => _client;
    }
}
