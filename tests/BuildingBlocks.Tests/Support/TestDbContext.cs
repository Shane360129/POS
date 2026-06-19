using InvenFlow.BuildingBlocks.Auditing;
using InvenFlow.BuildingBlocks.Messaging;
using InvenFlow.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace InvenFlow.BuildingBlocks.Tests.Support;

public sealed class TestEntity : IAuditable
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed record SampleEvent(string Text) : IntegrationEvent;

public sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> Items => Set<TestEntity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<TestEntity>().ToTable("TestEntity");
        b.Entity<AuditLog>().ToTable("AuditLog");
        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("Outbox");
            e.HasIndex(x => x.EventId).IsUnique();
        });
    }
}
