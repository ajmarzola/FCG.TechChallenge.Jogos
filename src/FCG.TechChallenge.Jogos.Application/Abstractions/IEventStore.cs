using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.TechChallenge.Jogos.Application.Abstractions
{
    public interface IEventStore
    {
        Task<int> AppendAsync(string streamId, int expectedVersion, IEnumerable<object> events, CancellationToken ct);
        Task<IReadOnlyList<object>> LoadAsync(string streamId, CancellationToken ct);
    }
}
