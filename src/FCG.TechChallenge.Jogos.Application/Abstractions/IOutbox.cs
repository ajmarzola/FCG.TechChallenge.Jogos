using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Application.Abstractions
{
    public interface IOutbox
    {
        Task EnqueueAsync(string type, string payload, CancellationToken ct);
    }
}
