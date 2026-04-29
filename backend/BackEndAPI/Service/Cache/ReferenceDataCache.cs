using BackEndAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace BackEndAPI.Service.Cache;

public class ReferenceDataCache : IReferenceDataCache
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(1);

    private readonly IMemoryCache _cache;
    private readonly IHubContext<ReferenceDataHub> _hub;
    private readonly ILogger<ReferenceDataCache> _logger;

    /// <summary>
    /// Version tracker per entity type. Atomic increment. Reset về 1 khi process restart
    /// — chấp nhận client cache stale 1 chu kỳ poll/SignalR (bình thường &lt; 1s).
    /// </summary>
    private static readonly ConcurrentDictionary<string, long> _versions = new(StringComparer.OrdinalIgnoreCase);

    public ReferenceDataCache(
        IMemoryCache cache,
        IHubContext<ReferenceDataHub> hub,
        ILogger<ReferenceDataCache> logger)
    {
        _cache = cache;
        _hub = hub;
        _logger = logger;
    }

    public async Task<T> GetOrSetAsync<T>(string entityType, Func<Task<T>> factory, CancellationToken ct = default)
    {
        var version = GetVersion(entityType);
        var cacheKey = $"refdata:{entityType}:v{version}";

        if (_cache.TryGetValue<T>(cacheKey, out var cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit: {Key}", cacheKey);
            return cached;
        }

        var data = await factory();
        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DefaultTtl,
            SlidingExpiration = TimeSpan.FromMinutes(15),
            Size = 1
        };
        _cache.Set(cacheKey, data, entryOptions);
        _logger.LogDebug("Cache miss → cached: {Key}", cacheKey);
        return data;
    }

    public long GetVersion(string entityType)
        => _versions.GetOrAdd(entityType, _ => 1);

    public async Task InvalidateAsync(string entityType, CancellationToken ct = default)
    {
        // Tăng version → cache key thay đổi → cache cũ bị orphan (sẽ tự expire).
        var newVersion = _versions.AddOrUpdate(entityType, 2, (_, old) => old + 1);

        _logger.LogInformation("Invalidate cache: entity={Entity} newVersion={Version}", entityType, newVersion);

        // Broadcast qua SignalR → client invalidate local cache + reload
        await _hub.Clients.All.SendAsync("ReferenceDataChanged", new
        {
            entityType,
            version = newVersion,
            timestamp = DateTime.UtcNow
        }, cancellationToken: ct);
    }
}
