using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Nest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Infrastructure.Elastic
{
    public sealed class ElasticBootService(IElasticClient es, IOptions<ElasticOptions> opt, ILogger<ElasticBootService> log) : IHostedService
    {
        private readonly IElasticClient _es = es;
        private readonly ILogger<ElasticBootService> _log = log;
        private readonly string _index = (opt.Value.Index ?? "jogos").Trim().TrimEnd('}', '/', ' ');

        public async Task StartAsync(CancellationToken ct)
        {
            var ping = await _es.PingAsync();
            if (!ping.IsValid)
            {
                _log.LogError("Elastic ping inválido: {info}", ping.DebugInformation);
                throw new InvalidOperationException("Não foi possível pingar o Elasticsearch.");
            }

            var exists = await _es.Indices.ExistsAsync(_index, ct: ct);
            if (!exists.IsValid)
            {
                _log.LogWarning("Falha ao checar existência do índice: {info}", exists.DebugInformation);
            }

            if (!exists.Exists)
            {
                var create = await _es.Indices.CreateAsync(_index, c => EsMappings.ConfigureIndex(c, _index), ct);
                if (!create.IsValid)
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
