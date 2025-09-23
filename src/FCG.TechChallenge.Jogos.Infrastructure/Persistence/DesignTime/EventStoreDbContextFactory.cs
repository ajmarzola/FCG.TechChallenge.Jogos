using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FCG.TechChallenge.Jogos.Infrastructure.Persistence.DesignTime
{
    public sealed class EventStoreDbContextFactory : IDesignTimeDbContextFactory<EventStoreDbContext>
    {
        public EventStoreDbContext CreateDbContext(string[] args)
        {
            // Descobre a raiz da solução a partir da pasta atual
            var basePath = Directory.GetCurrentDirectory();

            // Carrega appsettings (prioriza Development se existir)
            //var config = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", optional: true).AddJsonFile("appsettings.Development.json", optional: true).AddEnvironmentVariables().Build();

            // Ajuste a chave conforme você configurou (ex.: "Sql:ConnectionString")
            //var connectionString = config["Postgres"] ?? config.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string não encontrada. Defina 'Postgres' ou 'Postgres'.");

            var connectionString = "Host=localhost;Port=5432;Database=fcg_jogos;Username=postgres;Password=postgres;Pooling=true;Maximum Pool Size=50";
            var optionsBuilder = new DbContextOptionsBuilder<EventStoreDbContext>() .UseNpgsql(connectionString);

            return new EventStoreDbContext(optionsBuilder.Options);
        }
    }
}