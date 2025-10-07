namespace FCG.TechChallenge.Jogos.Infrastructure.Config.Options
{
    public sealed class ElasticOptions
    {
        public string Uri { get; set; } = default!;
        public string Index { get; set; } = "jogos";

        // Autenticação (use UM deles)
        public string? Username { get; set; }
        public string? Password { get; set; }

        // Elastic Cloud API Key
        public string? ApiKeyId { get; set; }
        public string? ApiKey { get; set; }
    }
}
