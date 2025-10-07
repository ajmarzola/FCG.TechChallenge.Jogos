namespace FCG.TechChallenge.Jogos.Infrastructure.Config.Options
{
    public sealed class ElasticOptions
    {
        public string Uri { get; set; } = default!;
        public string Index { get; set; } = "jogos";

        // Opção 1: Basic Auth
        public string? Username { get; set; }
        public string? Password { get; set; }

        // Opção 2/3: API Key (id + key) OU "id:key"
        public string? ApiKeyId { get; set; }
        public string? ApiKey { get; set; }
    }
}
