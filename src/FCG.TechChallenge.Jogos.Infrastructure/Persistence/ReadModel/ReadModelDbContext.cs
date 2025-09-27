using Microsoft.EntityFrameworkCore;

namespace FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel
{
    public sealed class ReadModelDbContext(DbContextOptions<ReadModelDbContext> options) : DbContext(options)
    {
        public DbSet<JogoRead> Jogos => Set<JogoRead>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.HasDefaultSchema("public");

            b.Entity<JogoRead>(e =>
            {
                e.ToTable("jogo_read");                  // tabela do read model
                e.HasKey(x => x.Id);

                e.Property(x => x.Nome).IsRequired();
                e.Property(x => x.Descricao);
                e.Property(x => x.Preco).HasColumnType("numeric(10,2)").IsRequired();
                e.Property(x => x.Categoria);

                e.Property(x => x.Version).HasDefaultValue(0);
                e.Property(x => x.CreatedUtc).HasDefaultValueSql("now()");
                e.Property(x => x.UpdatedUtc);

                e.HasIndex(x => x.Nome);
                e.HasIndex(x => x.Categoria);
            });
        }
    }
}