using FluentAssertions;
using InvenFlow.BuildingBlocks.Web;
using Xunit;

namespace InvenFlow.BuildingBlocks.Tests;

public sealed class PagingTests
{
    [Theory]
    [InlineData(1, 20, 0, 20)]
    [InlineData(3, 10, 20, 10)]
    [InlineData(0, 0, 0, 1)]
    [InlineData(2, 500, 200, 200)]
    public void Computes_skip_and_take(int page, int pageSize, int expectedSkip, int expectedTake)
    {
        var request = new PageRequest(page, pageSize);
        request.Skip.Should().Be(expectedSkip);
        request.Take.Should().Be(expectedTake);
    }
}
