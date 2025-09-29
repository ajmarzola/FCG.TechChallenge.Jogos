namespace FCG.TechChallenge.Jogos.Infrastructure.Config.Options
{
    public sealed class ElasticOptions
    {
        public string? Uri { get; set; }          // ex: "http://localhost:9200"
        public string? Index { get; set; }        // ex: "jogos"
        public string? Username { get; set; }     // se usar auth básica
        public string? Password { get; set; }     // se usar auth básica
    }
}
