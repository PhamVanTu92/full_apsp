using FluentAssertions;
using Moq;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Mẫu pattern unit test sử dụng Moq.
/// Đây là template để copy khi viết test cho service thực tế trong BackEndAPI.
/// Xem thêm: .claude/rules/testing.md
/// </summary>
public class SampleServiceTests
{
    public interface IGreetingProvider
    {
        string GetGreeting(string name);
    }

    public class GreetingService
    {
        private readonly IGreetingProvider _provider;
        public GreetingService(IGreetingProvider provider) => _provider = provider;
        public string Greet(string name)
            => string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Name required", nameof(name))
                : _provider.GetGreeting(name);
    }

    private readonly Mock<IGreetingProvider> _mockProvider = new();
    private readonly GreetingService _sut;

    public SampleServiceTests()
    {
        _sut = new GreetingService(_mockProvider.Object);
    }

    [Fact]
    public void Greet_WithValidName_ReturnsGreetingFromProvider()
    {
        _mockProvider.Setup(p => p.GetGreeting("Tu")).Returns("Xin chào Tu");

        var result = _sut.Greet("Tu");

        result.Should().Be("Xin chào Tu");
        _mockProvider.Verify(p => p.GetGreeting("Tu"), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Greet_WithEmptyName_Throws(string? name)
    {
        var act = () => _sut.Greet(name!);

        act.Should().Throw<ArgumentException>()
           .WithParameterName(nameof(name));
    }
}
