using FluentAssertions;
using InvenFlow.Modules.Mdm.Application;
using InvenFlow.Modules.Mdm.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InvenFlow.Modules.Mdm.Tests;

public sealed class NumberServiceTests
{
    private static MdmDbContext NewInMemoryDb()
    {
        // 共享記憶體 SQLite：連線開著期間資料庫即存在。
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<MdmDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new MdmDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task NextAsync_generates_unique_sequential_numbers()
    {
        using var db = NewInMemoryDb();
        var service = new NumberService(db);

        var numbers = new List<string>();
        for (var i = 0; i < 50; i++)
            numbers.Add(await service.NextAsync("PO"));

        numbers.Should().OnlyHaveUniqueItems();
        numbers.Should().HaveCount(50);
        numbers[0].Should().StartWith("PO");
    }

    [Fact]
    public async Task NextAsync_increments_within_same_key()
    {
        using var db = NewInMemoryDb();
        var service = new NumberService(db);

        var first = await service.NextAsync("SO");
        var second = await service.NextAsync("SO");

        first.Should().NotBe(second);
    }
}
