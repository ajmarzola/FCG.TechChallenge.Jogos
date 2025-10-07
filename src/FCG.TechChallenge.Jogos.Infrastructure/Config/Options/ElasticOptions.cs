namespace FCG.TechChallenge.Jogos.Infrastructure.Config.Options
{
    public sealed class ElasticOptions
    {
        public string CloudId { get; set; } = string.Empty;

        // Forma 1: dois campos
        public string? ApiKeyId { get; set; }
        public string? ApiKey { get; set; } // secret

        // Forma 2: um campo
        public string? ApiKeyBase64 { get; set; }

        public string Index { get; set; } = "jogos";
        public bool DisablePing { get; set; } = false;
    }
}
