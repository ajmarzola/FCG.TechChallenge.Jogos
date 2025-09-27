using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.TechChallenge.Jogos.Functions.Functions
{
    public class HotMetricsAggregator
    {
        private readonly ILogger _logger;

        public HotMetricsAggregator(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HotMetricsAggregator>();
        }

        [Function("HotMetricsAggregator")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
