using FCG.TechChallenge.Jogos.Infrastructure.Persistence.EventStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FCG.TechChallenge.Jogos.Infrastructure.Persistence.DesignTime
{
    public sealed class EventStoreDbContextFactory : IDesignTimeDbContextFactory<EventStoreDbContext>
    {
        public EventStoreDbContext CreateDbContext(string[] args)
        {
            var infraDir = Directory.GetCurrentDirectory();
            var apiDir = Path.GetFullPath(Path.Combine(infraDir, "..", "FCG.TechChallenge.Jogos.Api"));
            var config = new ConfigurationBuilder().SetBasePath(apiDir).AddJsonFile("appsettings.Development.json", optional: true).AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build();
            var cs = config.GetConnectionString("Postgres") ?? config["ConnectionStrings:Postgres"] ?? Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' não encontrada.");
            var options = new DbContextOptionsBuilder<EventStoreDbContext>().UseNpgsql(cs, npg => { npg.MigrationsHistoryTable("__EFMigrationsHistory", "public"); }).Options;
            return new EventStoreDbContext(options);
        }
    }
}