using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;

namespace FCG.TechChallenge.Jogos.Infrastructure.Elastic
{
    public sealed class ElasticBootService : IHostedService
    {
        private readonly ElasticsearchClient _es;
        private readonly ILogger<ElasticBootService> _log;
        private readonly string _index;

        public ElasticBootService(
            ElasticClientFactory factory,
            IOptions<ElasticsearchOptions> opt,
            ILogger<ElasticBootService> log)
        {
            _es = factory.Create();
            _log = log;
            _index = string.IsNullOrWhiteSpace(opt.Value.IndexName) ? "jogos" : opt.Value.IndexName.Trim().TrimEnd('}', '/', ' ');
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var ping = await _es.PingAsync(ct);
            if (!ping.IsSuccess())
            {
                _log.LogError("Elastic ping inválido: {info}", ping.DebugInformation);
                throw new InvalidOperationException("Não foi possível pingar o Elasticsearch.");
            }

            var exists = await _es.Indices.ExistsAsync(_index, ct);
            if (!exists.Exists)
            {
                // Definição do índice (v9). Se já tem EsIndexDefinition, use-a:
                var req = EsIndexDefinition.Build(_index);
                var create = await _es.Indices.CreateAsync(req, ct);
                if (!create.IsValidResponse)
                {
                    _log.LogError("Falha ao criar índice '{idx}': {info}", _index, create.DebugInformation);
                    throw new InvalidOperationException($"Falha ao criar índice '{_index}'.");
                }
                _log.LogInformation("Índice '{idx}' criado.", _index);
            }
            else
            {
                _log.LogInformation("Índice '{idx}' já existe.", _index);
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
