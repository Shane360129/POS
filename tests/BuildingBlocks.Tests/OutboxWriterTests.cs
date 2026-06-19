using FluentAssertions;
using InvenFlow.BuildingBlocks.Abstractions;
using InvenFlow.BuildingBlocks.Outbox;
using InvenFlow.BuildingBlocks.Tests.Support;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InvenFlow.BuildingBlocks.Tests;

public sealed class OutboxWriterTests
{
    private static TestDbContext NewDb()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
        var db = new TestDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task Add_persists_outbox_message_with_payload_and_event_id()
    {
        using var db = NewDb();
        var writer = new OutboxWriter<TestDbContext>(db, new SystemCurrentUser());
        var evt = new SampleEvent("hello");

        writer.Add(evt, aggregateId: 42, aggregateType: "Sample");
        await db.SaveChangesAsync();

        var message = await db.Outbox.SingleAsync();
        message.AggregateId.Should().Be(42);
        message.AggregateType.Should().Be("Sample");
        message.EventId.Should().Be(evt.EventId);
        message.Type.Should().Contain(nameof(SampleEvent));
        message.Payload.Should().Contain("hello");
        message.ProcessedAt.Should().BeNull();
    }
}
