using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.TechChallenge.Jogos.Functions.Functions
{
    public class PaymentsWebhook
    {
        private readonly ILogger<PaymentsWebhook> _logger;

        public PaymentsWebhook(ILogger<PaymentsWebhook> logger)
        {
            _logger = logger;
        }

        [Function("PaymentsWebhook")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
