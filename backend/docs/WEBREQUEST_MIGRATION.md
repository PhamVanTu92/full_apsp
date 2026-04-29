# Migration plan: HttpWebRequest → B1SLayer / HttpClient

## Bối cảnh

Code legacy trong `DocumentService.cs` (~50 vị trí) và `BusinessPartnerService.cs`
(~6 vị trí) dùng `HttpWebRequest` để gọi SAP B1 Service Layer:

```csharp
HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + "Login");
httpWebRequest.ContentType = "application/json";
httpWebRequest.Method = "POST";
// ... 15-20 dòng config
using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) { ... }
var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
```

`HttpWebRequest` đã obsolete từ .NET 6 (warning `SYSLIB0014`). Đếm 555 warnings còn
lại có ~200+ là từ pattern này.

## Vấn đề của code hiện tại

1. **Obsolete API**: SYSLIB0014 sẽ thành error trong .NET version sau
2. **Quản lý cookie SAP session thủ công**: code copy-paste 50 lần logic
   "if cookie null OR expired → login → parse Set-Cookie header"
3. **Không có connection pooling**: `WebRequest.Create` mỗi lần tạo socket mới →
   tốn TCP handshake với SAP
4. **Không OpenTelemetry trace**: HttpClient có instrumentation, WebRequest không
5. **Synchronous I/O đan xen async**: `httpWebRequest.GetRequestStream()` blocking
6. **Bypass TLS hardcoded**: dòng `ServerCertificateValidationCallback => true`
   xuất hiện 51 lần (đã wrap qua `TlsBypass.AcceptSelfSignedInDev` nhưng vẫn 51 chỗ)

## Mục tiêu

- 0 instance của `HttpWebRequest` còn trong codebase
- Mọi call SAP đi qua `SLConnection` (B1SLayer) — đã đăng ký DI sẵn
- Inbound trace ID propagate xuống outbound HttpClient → 1 traceId trace 1 request
  từ API → DB → SAP

## Cấu trúc đích

```
Service/Sap/
├── ISapClient.cs                  # Wrapper trên SLConnection
├── SapClient.cs                   # Implement, dùng B1SLayer
└── Models/
    ├── SapInvoice.cs              # DTO trả từ SAP
    ├── SapDraft.cs
    └── SapBPartner.cs
```

`ISapClient` cung cấp method semantically meaningful thay vì raw HTTP:

```csharp
public interface ISapClient
{
    // Drafts
    Task<List<SapDraft>> GetDraftsByMdhptAsync(string invoiceCode, CancellationToken ct);
    Task<int> CreateDraftAsync(SapOrderBP order, CancellationToken ct);

    // Invoices
    Task<List<SapInvoice>> GetInvoicesByCardCodeAsync(string cardCode, CancellationToken ct);
    Task<int> CreateInvoiceAsync(SapOrderBP order, CancellationToken ct);

    // Issues / VPKM
    Task<int> CreateIssueAsync(SapIssue issue, CancellationToken ct);
    Task<int> CreateVPKMAsync(SapVPKM vpkm, CancellationToken ct);
}
```

Service business (DocumentService) không bao giờ gọi SAP trực tiếp nữa, chỉ qua
`ISapClient`. Lợi:
- Mock trong test dễ dàng (không cần SAP server)
- Logic SAP request format tập trung 1 nơi
- Dễ retry / rate limit / circuit breaker (Polly)

## Thứ tự PR đề xuất (4-6 tuần, 1 PR/tuần)

### Week 1 — Setup scaffolding
- [ ] Tạo namespace `Service.Sap` với `ISapClient` skeleton
- [ ] Implement 1 method test: `GetDraftsByMdhptAsync` qua B1SLayer
- [ ] Unit test với mock `SLConnection`

### Week 2 — Migrate Drafts queries (read-only, low risk)
- [ ] Các vị trí `_context.Drafts?$filter=U_MDHPT eq ...` → `ISapClient.GetDraftsByMdhptAsync`
- [ ] ~5-7 vị trí trong DocumentService

### Week 3 — Migrate Login + session management
- [ ] B1SLayer auto-login nên 100% logic cookie thủ công có thể xoá
- [ ] Xoá class `Cookies` nội bộ
- [ ] Xoá ~20 lambda TLS bypass inline

### Week 4 — Migrate Invoices push (write — risk cao)
- [ ] `SyncToSapAsync` (push order draft → invoice)
- [ ] Test trên staging trước
- [ ] Verify SapDocEntry được set đúng (fix bug song song)

### Week 5 — Migrate Issues + VPKM
- [ ] `SyncIssueToSapAsync`
- [ ] `SyncVPKMToSapAsync`

### Week 6 — Cleanup
- [ ] Xoá hoàn toàn `using System.Net;` và `using System.Net.Http;` legacy
- [ ] Xoá class `Cookies`, `LoginInfor` nếu chỉ dùng nội bộ
- [ ] Confirm 0 SYSLIB0014 warning

## Risk & rollback

| Risk | Mitigation |
|---|---|
| B1SLayer behavior khác WebRequest cho edge case | Test trên staging với 10-20 doc trước khi migrate prod path |
| Format JSON khác → SAP reject payload | Capture raw body cũ và mới, diff |
| Performance regression (B1SLayer overhead) | Benchmark p99 latency before/after |

Rollback từng PR: revert commit → quay về HttpWebRequest. Vì migrate từng method,
mỗi PR độc lập.

## POC code mẫu (Week 1)

```csharp
public class SapClient : ISapClient
{
    private readonly SLConnection _sl;
    private readonly ILogger<SapClient> _logger;

    public SapClient(SLConnection sl, ILogger<SapClient> logger)
    {
        _sl = sl;
        _logger = logger;
    }

    public async Task<List<SapDraft>> GetDraftsByMdhptAsync(string invoiceCode, CancellationToken ct)
    {
        var safe = invoiceCode.Replace("'", "''");
        var response = await _sl.Request("Drafts")
            .Filter($"U_MDHPT eq '{safe}'")
            .Select("DocEntry,U_MDHPT,DocStatus")
            .GetAsync<DraftCollection>();
        return response?.Value ?? new();
    }

    private class DraftCollection { public List<SapDraft>? Value { get; set; } }
}
```

vs code legacy (~50 dòng cho cùng việc):
```csharp
HttpWebRequest httpWebRequestsdraft = (HttpWebRequest)WebRequest.Create(url + "Drafts?$filter=U_MDHPT eq '" + doc.InvoiceCode + "'&$select = DocEntry,U_MDHPT");
httpWebRequestsdraft.ContentType = "application/json";
httpWebRequestsdraft.Method = "GET";
httpWebRequestsdraft.KeepAlive = true;
httpWebRequestsdraft.ServerCertificateValidationCallback += BackEndAPI.Service.Sync.Security.TlsBypass.AcceptSelfSignedInDev();
httpWebRequestsdraft.Headers.Add("B1S-WCFCompatible", "true");
// ... 20 dòng nữa
```

→ Code mới gọn hơn ~80%, có cancellation token, có OpenTelemetry trace, không
manual session management, không injection vulnerability (escape `'`).

## Đo lường tiến độ

| Metric | Hiện tại | Mục tiêu sau migration |
|---|---|---|
| `HttpWebRequest` instances | ~50 | 0 |
| SYSLIB0014 warnings | ~200 | 0 |
| Total warnings | 508 | < 300 |
| Average lines per SAP call site | ~30 | ~5 |
| Test coverage trên SAP integration | 0% | 60%+ |
