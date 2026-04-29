# Reference Data Cache — chiến lược cache 3 tầng

Dữ liệu reference (Brand, Industry, ItemGroup, Region, BPSize, AppSetting...) được client gọi **liên tục** trên mọi page load nhưng **ít thay đổi** (chỉ admin sửa 1-2 lần/tuần). Tài liệu này mô tả giải pháp cache toàn bộ stack.

## Kiến trúc

```
┌─ Client (browser/mobile) ───────────────────────────────────────┐
│  ① Local cache (IndexedDB / AsyncStorage) lưu data + ETag       │
│  ② Mỗi GET gửi header: If-None-Match: <ETag đã lưu>             │
│  ③ SignalR connection → hub /api/hubs/reference-data            │
│     ↳ nhận event "ReferenceDataChanged" → invalidate local      │
└──────┬───────────────────────────────────┬──────────────────────┘
       │ HTTP                              │ WebSocket
       ▼                                   ▼
┌─ Backend ───────────────────────────────────────────────────────┐
│  A. ETagMiddleware: hash response body, so If-None-Match:       │
│     • match → 304 Not Modified (body rỗng, save bandwidth)      │
│     • miss  → set ETag header + Cache-Control: max-age=300      │
│  B. ReferenceDataCache (IMemoryCache server-side):              │
│     • GetOrSetAsync(key, factory) → DB query 1 lần / TTL        │
│     • TTL: 1 giờ absolute, 15 phút sliding                      │
│  C. ReferenceDataHub: khi admin update → broadcast SignalR      │
│     payload: { entityType, version, timestamp }                 │
└─────────────────────────────────────────────────────────────────┘
```

## Lợi ích

- **Server load**: lần đầu query DB, các lần sau ~3 phút ít nhất sẽ hit cache → **giảm 99% query DB** cho reference endpoints
- **Bandwidth**: Browser tự cache 5 phút, client còn ETag → 304 không truyền body → **tiết kiệm bandwidth**
- **Real-time**: admin cập nhật brand → tất cả client active được push event ngay → reload data mới
- **Dev experience**: developer chỉ thêm 2 dòng (`CacheableReferenceData` + `_cache.GetOrSetAsync`)

## Endpoints áp dụng được

Đếm từ frontend monitor:
| Endpoint | Tần suất gọi | Phù hợp cache? |
|---|---|---|
| `GET /api/brand/getall` | Mỗi page load | ✅ |
| `GET /api/industry/getall` | Mỗi page load | ✅ |
| `GET /api/industry/getallHiarchy` | Filter dropdown | ✅ |
| `GET /api/ItemGroup` | Mỗi page load | ✅ |
| `GET /api/BPSize/getall` | Form đăng ký BP | ✅ |
| `GET /api/regions?skip=0&limit=10000` | Address picker | ✅ |
| `GET /api/appsetting` | Page load + config | ✅ |
| `GET /api/Customer/{id}/debt` | Dashboard | ⚠ Per-customer, cache key phức tạp hơn |

## Cách áp dụng cho 1 endpoint

### Backend (3 thay đổi nhỏ)

**1. Inject `IReferenceDataCache` vào controller:**

```csharp
public class BrandController : Controller
{
    private readonly IBrandService _service;
    private readonly IReferenceDataCache _cache;

    public BrandController(IBrandService service, IReferenceDataCache cache)
    {
        _service = service;
        _cache = cache;
    }
}
```

**2. Wrap GET endpoint với cache + đánh dấu cacheable:**

```csharp
[HttpGet("getall")]
[CacheableReferenceData]   // ← bật ETag
public async Task<IActionResult> GetAll()
{
    var items = await _cache.GetOrSetAsync(ReferenceEntities.Brand, async () =>
    {
        var (data, mess) = await _service.GetAllAsync();
        if (mess != null) throw new Exception(mess.Errors);
        return data;
    });
    return Ok(items);
}
```

**3. Invalidate sau Add/Update/Delete:**

```csharp
[HttpPost("add")]
public async Task<IActionResult> Create(Brand model)
{
    var result = await _service.AddAsync(model);
    await _cache.InvalidateAsync(ReferenceEntities.Brand);  // ← push SignalR event
    return Ok(result);
}
```

### Frontend (JavaScript / TypeScript)

**1. Setup SignalR connection để nhận invalidation event:**

```typescript
import * as signalR from '@microsoft/signalr';

const hub = new signalR.HubConnectionBuilder()
  .withUrl('/api/hubs/reference-data', {
    accessTokenFactory: () => localStorage.getItem('accessToken')!,
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000])
  .build();

hub.on('ReferenceDataChanged', (e: { entityType: string; version: number; timestamp: string }) => {
  console.log('Reference data invalidated:', e);
  localCache.delete(`refdata:${e.entityType}`);
  // Optional: refetch ngay
  if (e.entityType === 'brand') refetchBrands();
});

await hub.start();
```

**2. Wrapper fetch với ETag support:**

```typescript
class CachedApi {
  private cache = new Map<string, { etag: string; data: any }>();

  async get<T>(url: string): Promise<T> {
    const cached = this.cache.get(url);
    const headers: Record<string, string> = {};
    if (cached) headers['If-None-Match'] = cached.etag;

    const res = await fetch(url, { headers });

    if (res.status === 304 && cached) {
      // Server xác nhận data chưa thay đổi
      return cached.data as T;
    }

    const etag = res.headers.get('ETag');
    const data = await res.json();
    if (etag) {
      this.cache.set(url, { etag, data });
    }
    return data as T;
  }

  invalidate(prefix: string) {
    for (const key of this.cache.keys()) {
      if (key.includes(prefix)) this.cache.delete(key);
    }
  }
}

// Tích hợp với SignalR:
hub.on('ReferenceDataChanged', (e) => api.invalidate(e.entityType));
```

**3. Persistent cache (optional, IndexedDB):**

```typescript
import { openDB } from 'idb';

const db = await openDB('refdata', 1, {
  upgrade(db) { db.createObjectStore('cache', { keyPath: 'url' }); }
});

// Save khi fetch xong
await db.put('cache', { url, etag, data, timestamp: Date.now() });

// Load khi app start
const persisted = await db.get('cache', url);
if (persisted && Date.now() - persisted.timestamp < 24 * 3600 * 1000) {
  // Dùng làm initial cache
  api.cache.set(url, { etag: persisted.etag, data: persisted.data });
}
```

## Kiểm thử

### Test ETag (curl)

```bash
# Lần 1 — server trả 200 + ETag
curl -i https://localhost:7238/api/brand/getall
# HTTP/1.1 200 OK
# ETag: "ABC123..."
# Cache-Control: private, max-age=300

# Lần 2 — gửi If-None-Match → 304 Not Modified (body rỗng)
curl -i -H 'If-None-Match: "ABC123..."' https://localhost:7238/api/brand/getall
# HTTP/1.1 304 Not Modified
```

### Test SignalR push (browser console)

```javascript
const hub = new signalR.HubConnectionBuilder()
  .withUrl('/api/hubs/reference-data', { accessTokenFactory: () => 'YOUR_JWT' })
  .build();
hub.on('ReferenceDataChanged', console.log);
await hub.start();

// Trên 1 tab khác, admin POST /api/brand/add
// → Tab này log: { entityType: 'brand', version: 5, timestamp: '...' }
```

### Test server cache hiệu quả

```bash
# Lần 1 — DB query
time curl https://localhost:7238/api/brand/getall  # ~150ms

# Lần 2 trong 1 giờ — IMemoryCache hit
time curl https://localhost:7238/api/brand/getall  # ~5ms
```

## Lưu ý vận hành

### Multi-instance (cluster mode)

`IMemoryCache` là **per-process**. Khi scale ra 3 instance:
- Mỗi instance có cache riêng → 3× memory nhưng OK với reference data nhỏ
- SignalR backplane: cần Redis/Azure SignalR Service để event broadcast tới mọi client trên mọi instance.

  ```csharp
  builder.Services.AddSignalR().AddStackExchangeRedis(redisConn);
  ```

### Cache stampede

Khi cache hết hạn cùng lúc với request burst → N request cùng query DB. `IMemoryCache` không có lock built-in.

**Fix nếu thấy load spike**: dùng `LazyCache` library hoặc tự lock `SemaphoreSlim` per key.

### TTL quá dài → stale data

Default 1h absolute, 15m sliding. Kết hợp với SignalR push → admin update sẽ invalidate ngay. Nếu SignalR fail (firewall, etc.) → max stale 1 giờ.

### Endpoint mutate quên invalidate

→ Client thấy data cũ đến khi cache expire (1 giờ). **Solution**: code review checklist + integration test.

## Checklist code review

- [ ] Endpoint GET có `[CacheableReferenceData]` attribute
- [ ] Endpoint GET wrap với `_cache.GetOrSetAsync(...)`
- [ ] Mọi POST/PUT/DELETE liên quan entity gọi `_cache.InvalidateAsync(...)`
- [ ] Entity type dùng constant từ `ReferenceEntities` class (tránh magic string)
- [ ] Frontend đã subscribe SignalR event `ReferenceDataChanged`

## References

- [`Service/Cache/IReferenceDataCache.cs`](../BackEndAPI/Service/Cache/IReferenceDataCache.cs)
- [`Service/Cache/ReferenceDataCache.cs`](../BackEndAPI/Service/Cache/ReferenceDataCache.cs)
- [`Hubs/ReferenceDataHub.cs`](../BackEndAPI/Hubs/ReferenceDataHub.cs)
- [`Middleware/ETagMiddleware.cs`](../BackEndAPI/Middleware/ETagMiddleware.cs)
- [`Controllers/BrandController.cs`](../BackEndAPI/Controllers/BrandController.cs) (POC mẫu)
