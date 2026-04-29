namespace BackEndAPI.Service.Cache;

/// <summary>
/// Cache reference data (Brand, Industry, ItemGroup, Region, BPSize, AppSetting...)
/// — dữ liệu ít thay đổi nhưng được client gọi liên tục.
///
/// Hoạt động:
///   1. Endpoint <c>GET /api/brand</c> → service kiểm tra IMemoryCache, hit → return.
///   2. Mỗi entity type có 1 version (long, monotonic increment).
///   3. ETag = "{entityType}:{version}". Client gửi <c>If-None-Match</c> → middleware
///      so sánh, match thì trả 304 Not Modified, không serialize lại.
///   4. Khi admin CRUD entity → IncrementVersion → SignalR broadcast version mới
///      → client gọi lại API (nhận data mới + ETag mới).
///
/// Key entity types được manage tập trung trong <see cref="ReferenceEntities"/>.
/// </summary>
public interface IReferenceDataCache
{
    /// <summary>Lấy hoặc tạo cache cho entity. Tự động tag với version hiện tại.</summary>
    Task<T> GetOrSetAsync<T>(string entityType, Func<Task<T>> factory, CancellationToken ct = default);

    /// <summary>Trả về version hiện tại của entity type. Tăng monotonic.</summary>
    long GetVersion(string entityType);

    /// <summary>Tăng version + invalidate cache + broadcast qua SignalR.</summary>
    Task InvalidateAsync(string entityType, CancellationToken ct = default);
}

/// <summary>
/// Constants cho các entity type được cache. Tránh magic strings.
/// </summary>
public static class ReferenceEntities
{
    public const string Brand = "brand";
    public const string Industry = "industry";
    public const string ItemGroup = "item-group";
    public const string Region = "region";
    public const string BPSize = "bp-size";
    public const string AppSetting = "app-setting";
    public const string Packing = "packing";
    public const string PaymentMethod = "payment-method";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Brand, Industry, ItemGroup, Region, BPSize, AppSetting, Packing, PaymentMethod
    };
}
