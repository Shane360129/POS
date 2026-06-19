using FluentAssertions;
using InvenFlow.BuildingBlocks.Abstractions;
using InvenFlow.BuildingBlocks.Auditing;
using InvenFlow.BuildingBlocks.Tests.Support;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InvenFlow.BuildingBlocks.Tests;

public sealed class AuditSaveChangesInterceptorTests
{
    private static TestDbContext NewDb()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new AuditSaveChangesInterceptor(new SystemCurrentUser()))
            .Options;
        var db = new TestDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task Writes_create_audit_for_auditable_entity()
    {
        using var db = NewDb();
        db.Items.Add(new TestEntity { Name = "alpha" });
        await db.SaveChangesAsync();

        var audits = await db.AuditLogs.ToListAsync();
        audits.Should().ContainSingle(a => a.Action == "Create" && a.EntityType == nameof(TestEntity));
    }

    [Fact]
    public async Task Writes_update_audit_with_before_and_after()
    {
        using var db = NewDb();
        var entity = new TestEntity { Name = "before" };
        db.Items.Add(entity);
        await db.SaveChangesAsync();

        entity.Name = "after";
        await db.SaveChangesAsync();

        var update = await db.AuditLogs.SingleOrDefaultAsync(a => a.Action == "Update");
        update.Should().NotBeNull();
        update!.After.Should().Contain("after");
        update.Before.Should().Contain("before");
    }
}
