using FluentAssertions;
using Xunit;

namespace BackEndAPI.Tests.Unit;

/// <summary>
/// Smoke test xác nhận xUnit + FluentAssertions hoạt động.
/// Khi chạy `dotnet test` lần đầu, đây là bài test phải pass.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void TestFramework_IsConfiguredCorrectly()
    {
        var sut = 1 + 1;
        sut.Should().Be(2);
    }

    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    public void Theory_RunsMultipleInputs(int a, int b, int expected)
    {
        (a + b).Should().Be(expected);
    }
}
