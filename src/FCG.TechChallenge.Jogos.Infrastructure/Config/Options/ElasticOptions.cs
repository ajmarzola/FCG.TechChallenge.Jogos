namespace FCG.TechChallenge.Jogos.Infrastructure.Config.Options
{
    public sealed class ElasticOptions
    {
        public string CloudId { get; set; } = string.Empty;
        public string ApiKeyId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        public string Index { get; set; } = "jogos";
        public bool DisablePing { get; set; } = false;
    }
}
