# BACKEND CODE REVIEW — APSP

> **Ngày review:** 2026-04-29  
> **Phạm vi:** `E:\APSP\FULL_PROJECT\backend\` (BackEndAPI + ClassLibrary + Area)  
> **Phương pháp:** Static analysis (đọc code trực tiếp), không chạy runtime, không sửa.  
> **Tổ chức:** Sắp xếp theo mức độ nghiêm trọng — Critical → High → Medium → Low.

---

## 📊 Bảng tổng hợp

| Mức độ | Bảo mật | Chất lượng code | Performance | Testing | Tổng |
|---|---|---|---|---|---|
| **Critical** | 2 | 2 | 1 | 1 | **6** |
| **High** | 3 | 5 | 4 | 0 | **12** |
| **Medium** | 4 | 4 | 2 | 1 | **11** |
| **Low** | 0 | 2 | 0 | 0 | **2** |

---

## 🔴 CRITICAL (6)

### C-1. Hardcoded production credentials trong `appsettings.json`
**File:** [backend/BackEndAPI/appsettings.json](backend/BackEndAPI/appsettings.json) — dòng 19-22, 38, 45-46, 61-63, 78-79  
**Loại:** Bảo mật

```json
"SmtpSettings": {
  "Username": "noreply@apsaigonpetro.com.vn1",
  "Password": "N0re@PSP2468"          // ❌ plaintext SMTP password
},
"ZaloTokenConfig": {
  "RefreshToken": "ksDbm462CZs16-tNKPPugFQRQSH1dbUuhxhqIAJYtwIgn...",
  "SecretKey":    "YmBMKn1XM9sOHGYD1ixNQ"
},
"PayoneConfig":  { "AccessCode": "6BEB2566", "HashCode": "6D0870CDE5F24F34F3915FB0045120D6" },
"VnpaySettings": { "VnpHashSecret": "K8FMCVHPSMK88KS2GWSKJARQ9PFR6PFR" }
```

**Vì sao nguy hiểm:** SMTP password hợp lệ, Zalo OA refresh token, payment gateway hash secrets được commit thẳng vào git. Bất kỳ ai clone repo đều có thể gửi email giả mạo, gọi Zalo OA API, hoặc tạo giao dịch thanh toán giả. Nếu repo từng public dù chỉ 1 phút, các secret này coi như đã lộ vĩnh viễn.

**Đề xuất:** Chuyển toàn bộ secrets sang User Secrets (dev) và environment variables / Azure Key Vault (prod). Rotate ngay lập tức tất cả các credential trên (không có cách "un-leak"). Scan git history với gitleaks/git-secrets.

```csharp
// Program.cs
var smtpPassword = builder.Configuration["SmtpSettings:Password"]
    ?? throw new InvalidOperationException("SmtpSettings:Password chưa được cấu hình");
```

---

### C-2. SQL Injection trong `ApprovalTemplateService`
**File:** [backend/BackEndAPI/Service/Approval/ApprovalTemplateService.cs:171](backend/BackEndAPI/Service/Approval/ApprovalTemplateService.cs)  
**Loại:** Bảo mật

```csharp
_context.Database.ExecuteSqlRaw(
    "DELETE FROM m2m_ApprovalOWTM WHERE OWTMId = {0}", items.Id);
```

**Vì sao nguy hiểm:** `ExecuteSqlRaw` với `{0}` placeholder thực ra **vẫn parameterized** khi `items.Id` là numeric — nhưng pattern này dễ bị bắt chước sai ở chỗ khác (ví dụ với string concat). Đây là code smell mạnh và cần được thay bằng API an toàn hơn.

**Đề xuất:** Dùng EF Core LINQ — không cần raw SQL.

```csharp
var rows = _context.m2m_ApprovalOWTM.Where(x => x.OWTMId == items.Id);
_context.m2m_ApprovalOWTM.RemoveRange(rows);
await _context.SaveChangesAsync();
```

---

### C-3. `[AllowAnonymous]` đè `[PrivilegeRequirement]` — Bypass authorization toàn diện
**Files & dòng:**
- [Controllers/ApprovalTemplateController.cs:26-29, 54-56, 77-79, 93-95](backend/BackEndAPI/Controllers/ApprovalTemplateController.cs)
- [Controllers/PaymentController.cs:29-31, 56-58](backend/BackEndAPI/Controllers/PaymentController.cs)
- [Controllers/ApprovalController.cs:37-38, 72-73](backend/BackEndAPI/Controllers/ApprovalController.cs)
- [Controllers/CustomerController.cs:76-78, 107-145](backend/BackEndAPI/Controllers/CustomerController.cs)

**Loại:** Bảo mật

```csharp
[AllowAnonymous]                                  // ❌ thắng
[HttpPost]
[Route("add")]
[PrivilegeRequirement("ApprovalTemplate.Create")]  // bị bỏ qua
public async Task<IActionResult> Create(OWTM model)
```

**Vì sao nguy hiểm:** Trong ASP.NET Core, `[AllowAnonymous]` luôn override mọi authorization filter. Hậu quả là **bất kỳ ai (không cần token)** đều có thể tạo approval template, gọi Payment endpoint, sync customer data. Đây là privilege bypass nghiêm trọng nhất có thể có trong một API.

**Đề xuất:** Xoá `[AllowAnonymous]` khỏi tất cả các endpoint nhạy cảm. Nếu thật sự cần public (ví dụ webhook), phải xoá luôn `[PrivilegeRequirement]` và verify bằng signature.

---

### C-4. `.Result` blocking trong async login flow → deadlock risk
**File:** [backend/BackEndAPI/Service/Account/AccountService.cs:629](backend/BackEndAPI/Service/Account/AccountService.cs)  
**Loại:** Chất lượng code (async/await)

```csharp
var check = _userManager.CheckPasswordAsync(user, login.Password);
if (check.Result != true)        // ❌ blocking trên async context
{
    mes.Errors = "Người dùng hoặc mật khẩu không đúng";
    return (null, null, mes);
}
```

**Vì sao nguy hiểm:** Login là hot path. `.Result` block thread pool thread, dưới load cao có thể dẫn đến thread pool exhaustion. Ngoài ra trong một số sync context vẫn có nguy cơ deadlock thực sự.

**Đề xuất:**

```csharp
if (!await _userManager.CheckPasswordAsync(user, login.Password))
{
    mes.Errors = "Người dùng hoặc mật khẩu không đúng";
    return (null, null, mes);
}
```

---

### C-5. `DangerousAcceptAnyServerCertificateValidator` trong DocumentService
**File:** [backend/BackEndAPI/Service/Document/DocumentService.cs:137-142](backend/BackEndAPI/Service/Document/DocumentService.cs)  
**Loại:** Chất lượng code / Bảo mật

```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
```

**Vì sao nguy hiểm:** Chấp nhận **mọi** TLS cert (kể cả self-signed/expired/forged) — mở MITM attack toàn phần với SAP server. Mặc dù `Program.cs` có guard `TlsBypass.IsEnabled` chỉ cho dev, riêng đoạn handler này trong DocumentService **không** check guard đó — luôn bật.

**Đề xuất:** Bọc trong điều kiện `TlsBypass.IsEnabled`, hoặc tốt hơn là pin certificate / dùng CA tin cậy của SAP server.

---

### C-6. Test coverage gần như bằng 0 ở các module quan trọng (tài chính)
**Phạm vi:** [backend/BackEndAPI.Tests/](backend/BackEndAPI.Tests/)  
**Loại:** Testing

| Module | Tests có | Rủi ro |
|---|---|---|
| Authentication / JWT | ❌ 0 | Login bypass, token rotation lỗi |
| Payment (VNPay / OnePay / Zalo) | ❌ 0 | **Tổn thất tài chính** |
| Approval Workflow V2 | ❌ 0 | Vượt cấp duyệt, đơn sai luồng |
| SAP Sync | ❌ 0 | Dữ liệu lệch giữa APSP & SAP |
| Encryption / Decryption | ❌ 0 | Secret giải mã sai → crash khởi động |
| Promotion Calculation | ❌ 0 | Tính sai khuyến mãi → tổn thất |

**Estimate:** ~11 test class / ~75 controller + ~47 service ≈ **<5% coverage**.

**Đề xuất:** Ưu tiên viết test cho Payment trước (rủi ro tài chính trực tiếp), sau đó Approval V2 engine, rồi SAP sync. Bổ sung integration test bằng `WebApplicationFactory<Program>`.

---

## 🟠 HIGH (12)

### H-1. Password generation dùng `System.Random` (không cryptographic)
**File:** [backend/BackEndAPI/Service/Account/AccountService.cs:114-151](backend/BackEndAPI/Service/Account/AccountService.cs)  
**Loại:** Bảo mật

```csharp
Random rand = new Random();   // ❌ predictable, seed by clock
for (int i = password.Length; i < length; i++)
    password.Append(allChars[rand.Next(allChars.Length)]);
```

**Vì sao:** `System.Random` không cryptographically secure. Nếu attacker biết thời gian tạo password (DB timestamp), có thể brute-force seed → suy ra password cho distributor.

**Đề xuất:** Dùng `RandomNumberGenerator` (System.Security.Cryptography).

```csharp
using var rng = RandomNumberGenerator.Create();
var bytes = new byte[length];
rng.GetBytes(bytes);
foreach (var b in bytes) sb.Append(allChars[b % allChars.Length]);
```

---

### H-2. File upload không validate extension / size / signature
**File:** [backend/BackEndAPI/Service/BusinessPartners/BusinessPartnerService.cs:1379-1437](backend/BackEndAPI/Service/BusinessPartners/BusinessPartnerService.cs)  
**Loại:** Bảo mật

```csharp
foreach (var file in files)
{
    if (file.Length > 0)   // ❌ không max size
    {
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);  // ❌ không whitelist
        using var fs = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fs);
    }
}
```

**Vì sao:** Có thể upload `.exe`, `.aspx`, file 10GB → DoS hoặc RCE nếu uploads folder được serve. Không check magic bytes → kẻ tấn công đặt extension `.jpg` cho file thực chất là script.

**Đề xuất:** Whitelist extension, giới hạn `file.Length`, validate magic bytes (4 byte đầu).

---

### H-3. SyncToSapAsync — N+1 và `.ToList()` toàn bảng Item
**File:** [backend/BackEndAPI/Service/Document/DocumentService.cs:520-528](backend/BackEndAPI/Service/Document/DocumentService.cs)  
**Loại:** Performance

```csharp
var itemKM = _context.Item.ToList();    // ❌ load toàn bộ Item (có thể 100k+ rows)
pp.ForEach(item => {
    var price = itemKM.FirstOrDefault(e => e.ItemCode == item.ItemCode)?.Price ?? 0;
    item.Price = price;
});
```

**Đề xuất:** Lọc trên DB và build dictionary cho lookup O(1).

```csharp
var codes = pp.Select(x => x.ItemCode).Distinct().ToList();
var dict = await _context.Item.AsNoTracking()
    .Where(i => codes.Contains(i.ItemCode))
    .ToDictionaryAsync(i => i.ItemCode);
pp.ForEach(p => { if (dict.TryGetValue(p.ItemCode, out var it)) p.Price = it.Price; });
```

---

### H-4. Cartesian explosion — PromotionService với Include lặp 4 lần cùng path
**File:** [backend/BackEndAPI/Service/Promotions/PromotionService.cs:169-191](backend/BackEndAPI/Service/Promotions/PromotionService.cs)  
**Loại:** Performance

```csharp
.Include(p => p.PromotionLine).ThenInclude(p => p.PromotionLineSub)
    .ThenInclude(p => p.PromotionItemBuy)
.Include(p => p.PromotionLine).ThenInclude(p => p.PromotionLineSub)   // ❌ lặp
    .ThenInclude(p => p.PromotionUnit)
.Include(p => p.PromotionLine).ThenInclude(p => p.PromotionLineSub)   // ❌ lặp
    .ThenInclude(p => p.PromotionLineSubSub).ThenInclude(p => p.PromotionSubItemAdd)
.Include(p => p.PromotionLine).ThenInclude(p => p.PromotionLineSub)   // ❌ lặp
    .ThenInclude(p => p.PromotionLineSubSub).ThenInclude(p => p.PromotionSubUnit)
```

**Vì sao:** SQL Server tạo JOIN cartesian → nếu Promotion=10, Line=10, Sub=5, SubSub=3 thì kết quả ~6,000 rows cho 10 promotion. RAM + bandwidth tăng 50× — 100×.

**Đề xuất:** `.AsSplitQuery()` ở đầu chain — EF Core sẽ tách thành nhiều SELECT thay vì 1 JOIN khổng lồ.

---

### H-5. Approval V2 GetAllAsync N+1
**File:** [backend/BackEndAPI/Service/Approval_V2/ApprovalWorkFlow/Service/ApprovalWorkFlowService.cs:46-54](backend/BackEndAPI/Service/Approval_V2/ApprovalWorkFlow/Service/ApprovalWorkFlowService.cs)  
**Loại:** Performance

```csharp
var result = await query.Include(...).ToListAsync();
foreach (var res in result)                                  // ❌ N+1
{
    var engine = _approvalWorkFlowFactory.GetEngine(...);
    var item = await engine.GetEntityAsync(res.ApprovalWorkFlowDocumentLines[0].DocId);
    res.ApprovalWorkFlowDocumentLines[0].DocObj = item;
}
```

**Đề xuất:** Thêm method `GetEntitiesAsync(List<int> docIds)` để batch load 1 query, build dictionary.

---

### H-6. N+1 trong Approval.cs (filter sau khi `.ToList()`)
**File:** [backend/BackEndAPI/Service/Approval/Approval.cs:99-108](backend/BackEndAPI/Service/Approval/Approval.cs)  
**Loại:** Performance

```csharp
var apts = context.WTM2.Include(...).ToList();           // ❌ load tất cả
var apt  = apts.FirstOrDefault(e =>
    e.OWTM.RUsers.Any(p => p.Id == approval.ActorId.Value)
    && e.OWTM.RUsers.Count != 0);                         // filter ở C#
```

**Đề xuất:** Đẩy filter vào `.Where(...)` trước `FirstOrDefaultAsync()`.

---

### H-7. `new HttpClient()` rải rác 25+ chỗ — không dùng `IHttpClientFactory`
**File:** [backend/BackEndAPI/Service/Report/ReportService.cs:163, 216, 296, 370](backend/BackEndAPI/Service/Report/ReportService.cs) (và nhiều file khác)  
**Loại:** Chất lượng code / Performance

```csharp
var client = new HttpClient();   // ❌ không pool connection
var resp   = await client.SendAsync(req);
```

**Vì sao:** `HttpClient` mới mỗi lần tạo socket mới, không pool, gây **socket exhaustion** dưới tải; `HttpClient` cũng implement `IDisposable` nhưng không thuộc loại nên dispose ngay → DNS không refresh.

**Đề xuất:** Đăng ký `services.AddHttpClient<ReportService>()` trong `Program.cs`, inject `IHttpClientFactory` hoặc typed client.

---

### H-8. God class — `DocumentService.cs` 9,313 dòng / 14 dependency
**File:** [backend/BackEndAPI/Service/Document/DocumentService.cs:113-116](backend/BackEndAPI/Service/Document/DocumentService.cs)  
**Loại:** Chất lượng code (SOLID)

- 9,313 dòng, 30+ public method, constructor inject 14 dependency.
- `SyncToSapAsync` dài 838 dòng (line 217-1054).
- 3 phiên bản gần giống nhau: `GetPaymentInfo` (1976-3862, ~1,887 dòng), `GetPaymentInfo1` (~345 dòng), `GetPaymentInfoNET` (~345 dòng).

**Đề xuất:** Tách theo bounded context: `DocumentCrudService`, `DocumentSyncService`, `DocumentPaymentCalculator`, `DocumentApprovalService`, `DocumentAttachmentService`. Gộp 3 phiên bản `GetPaymentInfo` thành 1 method với `enum PaymentCalculationMode`.

---

### H-9. Thiếu cache server-side cho master data
**File:** [backend/BackEndAPI/Program.cs:231-232](backend/BackEndAPI/Program.cs)  
**Loại:** Performance

```csharp
builder.Services.AddMemoryCache(opt => opt.SizeLimit = 1024);
builder.Services.AddSingleton<IReferenceDataCache, ReferenceDataCache>();
```

**Vì sao:** `ReferenceDataCache` đã có hạ tầng (ETag + SignalR invalidation) nhưng các endpoint Brand/Industry/Packing/UOM/ItemGroup vẫn truy DB mỗi request. Mỗi user load trang → ~5-10 master-data query không cần thiết.

**Đề xuất:** Wrap các GET endpoint master-data qua `IReferenceDataCache.GetOrAddAsync(...)`. TTL 1h. Backend đã broadcast `ReferenceDataChanged` qua SignalR khi admin sửa — chỉ cần consume.

---

### H-10. PayOne credentials bị đổ vào static field
**File:** [backend/BackEndAPI/Program.cs:112-116](backend/BackEndAPI/Program.cs)  
**Loại:** Chất lượng code / Bảo mật

```csharp
PayOne._config = builder.Configuration.GetSection("PayoneConfig").Get<PayOneConfig>();
string merchantId = PayOne._config.PayNow.Id;
```

**Vì sao:** Static mutable state chứa credential — khó test, race condition khi reload config, không tuân theo DI lifecycle. Một test set khác xong thì test sau bị ảnh hưởng.

**Đề xuất:** Đăng ký `IOptions<PayOneConfig>` qua `services.Configure<PayOneConfig>(...)` và inject vào service.

---

### H-11. `SaveChanges()` đồng bộ trong async context
**Files:**
- [Service/Document/DocumentService.cs:7284](backend/BackEndAPI/Service/Document/DocumentService.cs)
- [Service/Approval/Approval.cs:135](backend/BackEndAPI/Service/Approval/Approval.cs)

**Loại:** Chất lượng code

```csharp
_context.Approval.AddRange(approval);
_context.SaveChanges();            // ❌ sync
foreach (var item in approval) { ... }
await _context.SaveChangesAsync(); // mix sync/async
```

**Đề xuất:** Đổi `SaveChanges()` → `await SaveChangesAsync()` đồng bộ trong toàn flow.

---

### H-12. `.Count() > 0` thay vì `.Any()` — pattern lặp lại nhiều chỗ
**File:** [backend/BackEndAPI/Service/ItemMasterData/ItemService.cs:1215, 1422, 1970, 2026, 2156](backend/BackEndAPI/Service/ItemMasterData/ItemService.cs)  
**Loại:** Performance

```csharp
if (itemCode.Count() > 0)         // ❌ đếm hết rồi mới so sánh
    query = query.Where(...);
```

**Vì sao:** Trên `IEnumerable`/`IQueryable`, `Count()` thực thi đầy đủ; `Any()` short-circuit ngay khi gặp phần tử đầu tiên (SQL: `EXISTS` thay vì `COUNT(*)`).

**Đề xuất:** Replace toàn dự án: `Count() > 0` → `Any()`, `Count() == 0` → `!Any()`.

---

## 🟡 MEDIUM (11)

### M-1. Insecure deserialization không try-catch
**File:** [backend/BackEndAPI/Controllers/CustomerController.cs:421](backend/BackEndAPI/Controllers/CustomerController.cs)

```csharp
string[] noteArr = JsonConvert.DeserializeObject<string[]>(notes);
if (files.Count != noteArr!.Length)   // NRE nếu null
    return BadRequest("Note và file phai bang nhau");
```
JSON sai cú pháp → unhandled exception → 500. Wrap try-catch và validate null.

---

### M-2. SeedData dùng `ExecuteSqlRawAsync` cho `SET IDENTITY_INSERT`
**File:** [backend/BackEndAPI/Data/SeedData.cs:334, 344](backend/BackEndAPI/Data/SeedData.cs)

Không phải lỗ hổng (no user input), nhưng nên gói trong transaction rõ ràng và tách ra extension method để không lan ra chỗ khác.

---

### M-3. Thiếu `.AsNoTracking()` cho read-only queries
**File:** [backend/BackEndAPI/Service/ConfirmationSystems/ConfirmationDocumentService.cs:128](backend/BackEndAPI/Service/ConfirmationSystems/ConfirmationDocumentService.cs) (và nhiều file Service khác)

```csharp
.Where(d => d.CardId == CardId || CardId == 0).ToList();
```
Read-only nhưng vẫn track. Tốn RAM cho ChangeTracker. Thêm `.AsNoTracking()` cho mọi GET endpoint.

---

### M-4. Exception handling chỉ log `ex.Message`, mất context
**File:** [backend/BackEndAPI/Service/Document/DocumentService.cs:7315-7319](backend/BackEndAPI/Service/Document/DocumentService.cs)

```csharp
catch (Exception ex)
{
    mess.Errors = ex.Message;     // ❌ không log ex object, không có user/doc context
    transaction.Rollback();
}
```
Mất stack trace và inner exception → không debug được production. Nên `_logger.LogError(ex, "SyncToSapAsync failed for DocId={DocId} UserId={UserId}", id, userId)`.

---

### M-5. Magic numbers dày đặc — `ObjType` 198, 1250000001, 22, 12...
**File:** [backend/BackEndAPI/Service/Document/DocumentService.cs:1472-1493](backend/BackEndAPI/Service/Document/DocumentService.cs)

```csharp
if (ObjType == 198)         { notiType = "fc";   ... }
else if (ObjType == 1250000001) { codes = await GenerateDocument("YCLH",...); }
```
Đề xuất `enum DocumentType { ForecastPlan = 198, PickupRequest = 1250000001, ... }`.

---

### M-6. `AutoPointJob` còn dở dang
**File:** [backend/BackEndAPI/Service/Jobs/AutoPointJob.cs:24-37](backend/BackEndAPI/Service/Jobs/AutoPointJob.cs)

```csharp
.ToList();   // ❌ sync ToList trên DbSet trong job
// TODO: bật khi xác nhận business logic CalculatePoints đúng
```
Body bị disable, dùng `.ToList()` đồng bộ. Hoặc bật và viết test, hoặc remove khỏi schedule cho đến khi sẵn sàng.

---

### M-7. `[AllowAnonymous]` không bị conflict nhưng có nghi vấn ở `CustomerController` sync endpoints
**File:** [backend/BackEndAPI/Controllers/CustomerController.cs:107-145](backend/BackEndAPI/Controllers/CustomerController.cs)

Nhiều POST sync endpoint anonymous — không rõ có signature/IP whitelist hay không. Nếu dùng webhook từ SAP cần verify signature.

---

### M-8. `LogInformation` trong vòng lặp / hot path (rủi ro disk I/O)
**File:** [backend/BackEndAPI/Service/Jobs/AutoPointJob.cs:31](backend/BackEndAPI/Service/Jobs/AutoPointJob.cs) và một số sync job

Mỗi item log 1 dòng → khi job xử lý 100k record, file log nhảy lên hàng GB/ngày. Nên log aggregate (count, duration) thay vì per-item.

---

### M-9. Constructor inject quá nhiều dependency (>8) ở nhiều service
**Files:** DocumentService (14), DocumentService dependency tree → các service downstream cũng có ≥10 dep.  
**Đề xuất:** Refactor theo SRP, hoặc gom các dep liên quan thành facade service.

---

### M-10. Rate limit policies ở Program.cs có nhưng coverage chưa rõ
**File:** [backend/BackEndAPI/Program.cs:521-594](backend/BackEndAPI/Program.cs)

Login (5/min/IP), Read (60/min/IP), Write (20/min/user), Admin (2/min/IP) — tốt. Nhưng cần verify mỗi endpoint thực sự có `[EnableRateLimiting("policyName")]`. Một số controller có thể bị bỏ sót.

---

### M-11. Empty `BackEndAPI.Tests` project
**File:** [backend/BackEndAPI.Tests/](backend/BackEndAPI.Tests/)

Tồn tại nhưng gần như không có test thực chất. CI chạy `dotnet test` vẫn pass nhưng không test gì → cảm giác an toàn giả. Hoặc viết test, hoặc đánh dấu rõ "TBD" và set up coverage gate.

---

## 🟢 LOW (2)

### L-1. TODO/FIXME không tracked
**File:** [backend/BackEndAPI/Service/Jobs/AutoPointJob.cs:33](backend/BackEndAPI/Service/Jobs/AutoPointJob.cs)
```csharp
// TODO: bật khi xác nhận business logic CalculatePoints đúng
```
Comment lẻ → nên chuyển thành GitHub issue có owner + due date.

---

### L-2. Suppress `NU1902` cho MailKit CVE
**File:** [backend/BackEndAPI/BackEndAPI.csproj](backend/BackEndAPI/BackEndAPI.csproj)

```xml
<NoWarn>$(NoWarn);CS1591;NU1902</NoWarn>
```
MailKit/MimeKit 4.14.0 có CVE moderate. Tracking upstream là OK ngắn hạn, nhưng nên có cron job kiểm tra phiên bản fix và mở issue tự động.

---

## ✅ Điểm tích cực đã ghi nhận

- **JWT validation chuẩn** (Program.cs:361-373): Validate Issuer, Audience, Lifetime, Signing Key đều bật.
- **TLS bypass có guard environment** (Program.cs:601-608) — không bật mặc định production.
- **CORS** (Program.cs:376-403): Dev cho mọi origin, Prod whitelist từ config + throw nếu chưa cấu hình.
- **Password policy** (Program.cs:330-351): Yêu cầu hoa/thường/số/đặc biệt + lockout 5 lần.
- **Refresh token rotation** (AccountController.cs:142-187): Rotate + revoke chuẩn, có logout revoke.
- **Rate limiting** (Program.cs:521-594) — phân loại theo loại endpoint.
- **Quartz job** dùng `[DisallowConcurrentExecution]` (SyncJobBase.cs:19) — không overlap.
- **Serilog structured logging** — không thấy string interpolation trong log.
- **File I/O nói chung** dùng `using` đầy đủ — không thấy memory leak rõ ràng.
- **`UseHttpsRedirection()`** được bật (Program.cs:649).

---

## 🎯 Khuyến nghị thứ tự ưu tiên xử lý

| Sprint | Hạng mục | Thời lượng ước tính |
|---|---|---|
| **Ngay (P0)** | C-1 rotate & remove credentials, scan git history | 2h |
| **Ngay (P0)** | C-3 xoá `[AllowAnonymous]` trên endpoint nhạy cảm | 4h |
| **Ngay (P0)** | C-4 `.Result` → `await` ở login | 30min |
| **Tuần này (P1)** | C-2 SQL injection pattern, H-1 random password | 2h |
| **Tuần này (P1)** | H-3, H-4, H-5, H-6, H-12 — performance EF queries | 6h |
| **Tuần này (P1)** | C-5 TLS guard | 1h |
| **Sprint sau (P2)** | C-6 viết test cho Payment / Approval V2 / SAP sync | 40h |
| **Sprint sau (P2)** | H-7 IHttpClientFactory migration | 4h |
| **Sprint sau (P2)** | H-8 tách DocumentService god-class | 16-24h |
| **Sprint sau (P2)** | H-9 cache reference data | 3h |
| **Backlog (P3)** | M-1..M-11 + L-1, L-2 | rải rác |

**Ghi chú:** Báo cáo này là static analysis. Sau khi fix các Critical/High, chạy load test (k6/JMeter) để xác minh cải thiện, và security scan tự động (OWASP ZAP, gitleaks) để bắt regressions.
