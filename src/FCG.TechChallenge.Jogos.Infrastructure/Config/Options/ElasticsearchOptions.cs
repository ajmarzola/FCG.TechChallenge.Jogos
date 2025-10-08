using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Infrastructure.Config.Options
{
    public sealed class ElasticsearchOptions
    {
        public string? Uri { get; set; }
        public string? ApiKey { get; set; }      // Base64 (id:key) do Elastic Cloud
        public string? IndexName { get; set; }   // ex: "api-jogos-es"
    }
}
