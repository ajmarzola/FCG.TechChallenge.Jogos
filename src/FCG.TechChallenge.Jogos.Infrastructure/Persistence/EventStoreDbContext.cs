using Microsoft.EntityFrameworkCore;

namespace FCG.TechChallenge.Jogos.Infrastructure.Persistence
{
    public sealed class EventStoreDbContext : DbContext
    {
        public EventStoreDbContext(DbContextOptions<EventStoreDbContext> opt) : base(opt) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Schema padrão "public"
            b.HasDefaultSchema("public");

            b.Entity<EventRow>(cfg =>
            {
                cfg.ToTable("Events");
                cfg.HasKey(x => new { x.StreamId, x.Version });

                cfg.Property(x => x.StreamId).HasMaxLength(100).IsRequired();
                cfg.Property(x => x.Version).IsRequired();

                cfg.Property(x => x.EventId).IsRequired();
                cfg.Property(x => x.Type).HasMaxLength(200).IsRequired();

                // Em Postgres, use TEXT/JSONB; o provider cuida.
                cfg.Property(x => x.Data).IsRequired();       // mapeará para text por padrão
                cfg.Property(x => x.Metadata);                // text ou jsonb se você quiser

                // NOW() AT TIME ZONE 'UTC' → UTC
                cfg.Property(x => x.CreatedUtc)
                   .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')")
                   .IsRequired();

                // Índice alternativo por EventId
                cfg.HasIndex(x => x.EventId).IsUnique();
            });

            b.Entity<OutboxRow>(cfg =>
            {
                cfg.ToTable("OutboxMessages");
                cfg.HasKey(x => x.Id);

                cfg.Property(x => x.Type).HasMaxLength(200).IsRequired();
                cfg.Property(x => x.Payload).IsRequired();

                cfg.Property(x => x.CreatedUtc)
                   .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')")
                   .IsRequired();

                cfg.Property(x => x.ProcessedUtc);
                cfg.Property(x => x.Error);
            });
        }

        public DbSet<EventRow> Events => Set<EventRow>();
        public DbSet<OutboxRow> Outbox => Set<OutboxRow>();
    }

    public sealed class EventRow
    {
        public string StreamId { get; set; } = default!;
        public int Version { get; set; }
        public Guid EventId { get; set; }
        public string Type { get; set; } = default!;
        public string Data { get; set; } = default!;
        public string? Metadata { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public sealed class OutboxRow
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTime CreatedUtc { get; set; }
        public DateTime? ProcessedUtc { get; set; }
        public string? Error { get; set; }
    }
}
