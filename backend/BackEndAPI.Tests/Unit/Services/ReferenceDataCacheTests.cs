using BackEndAPI.Hubs;
using BackEndAPI.Service.Cache;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Tests cho ReferenceDataCache — cache layer cho reference data
/// (Brand, Industry, ItemGroup, ...) với version tracking và SignalR push.
/// </summary>
public class ReferenceDataCacheTests
{
    private static (ReferenceDataCache cache, Mock<IHubClients> hubClients) Create()
    {
        var mc = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
        var hub = new Mock<IHubContext<ReferenceDataHub>>();
        var clients = new Mock<IHubClients>();
        var allClients = new Mock<IClientProxy>();

        hub.Setup(h => h.Clients).Returns(clients.Object);
        clients.Setup(c => c.All).Returns(allClients.Object);

        return (new ReferenceDataCache(mc, hub.Object, NullLogger<ReferenceDataCache>.Instance), clients);
    }

    [Fact]
    public async Task GetOrSetAsync_FirstCall_InvokesFactory()
    {
        var (cache, _) = Create();
        int factoryCalls = 0;

        var result = await cache.GetOrSetAsync("test", () =>
        {
            factoryCalls++;
            return Task.FromResult("data1");
        });

        result.Should().Be("data1");
        factoryCalls.Should().Be(1);
    }

    [Fact]
    public async Task GetOrSetAsync_SecondCallSameKey_ReturnsCached_FactoryNotInvokedAgain()
    {
        var (cache, _) = Create();
        int factoryCalls = 0;
        Task<string> Factory() { factoryCalls++; return Task.FromResult("cached"); }

        await cache.GetOrSetAsync("test", Factory);
        await cache.GetOrSetAsync("test", Factory);
        await cache.GetOrSetAsync("test", Factory);

        factoryCalls.Should().Be(1, "cache hit phải skip factory");
    }

    [Fact]
    public async Task InvalidateAsync_IncrementsVersion_AndBroadcasts()
    {
        var (cache, hubClients) = Create();
        var v0 = cache.GetVersion("brand");

        await cache.InvalidateAsync("brand");

        cache.GetVersion("brand").Should().Be(v0 + 1);
        hubClients.Verify(c => c.All, Times.Once, "phải broadcast tới All clients");
    }

    [Fact]
    public async Task InvalidateAsync_NextGetOrSet_InvokesFactoryAgain()
    {
        var (cache, _) = Create();
        int factoryCalls = 0;
        Task<string> Factory() { factoryCalls++; return Task.FromResult($"v{factoryCalls}"); }

        await cache.GetOrSetAsync("test", Factory);
        await cache.InvalidateAsync("test");
        var result = await cache.GetOrSetAsync("test", Factory);

        factoryCalls.Should().Be(2);
        result.Should().Be("v2");
    }

    [Fact]
    public void GetVersion_DifferentEntities_HaveSeparateVersions()
    {
        var (cache, _) = Create();
        // Default = 1
        cache.GetVersion("brand").Should().Be(1);
        cache.GetVersion("industry").Should().Be(1);

        // Increment chỉ entity đó
        cache.InvalidateAsync("brand").GetAwaiter().GetResult();
        cache.GetVersion("brand").Should().BeGreaterThan(1);
        cache.GetVersion("industry").Should().Be(1, "industry version độc lập");
    }
}
