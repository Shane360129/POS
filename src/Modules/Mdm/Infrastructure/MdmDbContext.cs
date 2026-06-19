using InvenFlow.BuildingBlocks.Auditing;
using InvenFlow.BuildingBlocks.Outbox;
using InvenFlow.Modules.Mdm.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.Modules.Mdm.Infrastructure;

public sealed class MdmDbContext : DbContext
{
    public MdmDbContext(DbContextOptions<MdmDbContext> options) : base(options) { }

    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // schema 隔離（SQL Server）；SQLite 無 schema 概念會自動忽略，僅用表名。
        b.Entity<NumberSequence>(e =>
        {
            e.ToTable("NumberSequence", "mdm");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(20).IsRequired();
            e.Property(x => x.Period).HasMaxLength(8).IsRequired();
            e.HasIndex(x => new { x.Key, x.Period }).IsUnique();
        });

        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("Outbox", "app");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EventId).IsUnique();
            e.Property(x => x.AggregateType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Type).HasMaxLength(150).IsRequired();
        });

        b.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLog", "audit");
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(50).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
        });
    }
}
