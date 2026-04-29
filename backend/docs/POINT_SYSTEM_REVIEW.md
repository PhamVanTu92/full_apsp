# RFC — Review hệ thống điểm khách hàng (PointSetupService & related)

| Field | Value |
|---|---|
| **Status** | Draft |
| **Scope** | `Service/Promotions/PointSetupService.cs` (1168 dòng) + ExchangePointService + CustomerPointHistoryService + integration với DocumentService/BusinessPartnerService/PvService |
| **Phát hiện qua audit code 2026-04-28** |  |

## 1. Tóm tắt

Hệ thống điểm gồm 3 luồng chính: **tính điểm** khi đặt đơn, **trừ điểm** khi đổi quà (VPKM), **hoàn điểm** khi huỷ. Code đang hoạt động nhưng có **17 vấn đề nghiêm trọng** từ correctness (race condition, double-spend, sai logic FIFO khi hoàn) đến testability (1168 dòng God class, lock-in vào DateTime.Now, không inject được). RFC này phân tích và đề xuất refactor sang kiến trúc event-driven có audit trail bền vững.

## 2. Domain hiện tại

### 2.1 Entities

```
PointSetup              # Chương trình tích điểm
├── PointSetupCustomer  # Áp cho ai (Type=C/G/All)
└── PointSetupLine      # Quy tắc: bao nhiêu điểm/đơn vị
    ├── Industry        # Lọc theo ngành
    ├── Brands          # Lọc theo brand
    ├── Packings        # Lọc theo quy cách
    └── ItemType        # Lọc item cụ thể (Type=I) hoặc nhóm (Type=G)

ExchangePoint           # Đổi quà (catalog VPKM)
└── ExchangePointLine   # Item nào đổi bao nhiêu điểm

CustomerPointCycle      # Tài khoản điểm theo (Customer × PointSetup × period)
                        # EarnedPoint, RedeemedPoint, RemainingPoint, ExpiryDate

CustomerPoint           # 1 transaction (đơn hàng / đổi quà / huỷ)
└── CustomerPointLine   # Detail từng item, PointChange (+/-)

CalculatorPoint         # DTO input từ controller
└── CalculatorPointLine
```

### 2.2 Luồng hoạt động

```
A. ĐẶT ĐƠN HÀNG (ObjType=22, Status=DXN):
   DocumentService.CreateOrder()
     → PointSetupService.CalculatePoints(calPoint)
        → tạo CustomerPoint (AddPoint=true, Details với PointChange dương)
        → CHƯA cập nhật CustomerPointCycle (chỉ ghi lịch sử)

B. ĐÓNG ĐƠN (Status=DGH/DHT/DTT - thanh toán xong):
   PvService → PointSetupService.CalculatePointsCircle(docId, "CP", null)
     → cập nhật CustomerPointCycle: EarnedPoint += sum, RemainingPoint += sum

C. HUỶ ĐƠN HÀNG (Status=CXN/CTT):
   PvService → PointSetupService.CalculatePointsCircle(docId, "C", null)
     → cycle.RemainingPoint -= sum, EarnedPoint -= sum
     → tạo reverse CustomerPoint (PointChange âm)

D. ĐỔI QUÀ (ObjType=12, Status=DXN):
   DocumentService → PointSetupService.RedeemPoints(p, "CP")
     → FIFO: trừ từ cycle có ExpiryDate sớm nhất
     → cycle.RedeemedPoint += use, RemainingPoint -= use
     → tạo CustomerPoint (AddPoint=false, PointChange âm)

E. HUỶ ĐỔI QUÀ:
   PvService → PointSetupService.RedeemPoints(p, "C")
     → hoàn lại từng setup theo history grouped by setupId
```

## 3. Vấn đề chi tiết

### 🔴 CRITICAL — correctness

#### 3.1 Race condition trên `CustomerPointCycle.RemainingPoint`

```csharp
// RedeemPoints — FIFO loop
double usePoint = Math.Min(remainingToRedeem, cycle.RemainingPoint);
cycle.RedeemedPoint += usePoint;
cycle.RemainingPoint -= usePoint;
```

Nếu 2 request đổi quà gửi cùng lúc cho cùng customer (mobile + web đồng thời):
- T1 đọc `RemainingPoint = 100`
- T2 đọc `RemainingPoint = 100`
- T1 trừ 80 → save (`= 20`)
- T2 trừ 80 → save (`= 20`) ← OVERWRITE T1
- **Kết quả**: customer bị trừ 80 điểm nhưng đã đổi 2 quà 80 điểm = lỗ 80 điểm

→ Không có optimistic concurrency token (`[Timestamp]`/`RowVersion`) hay pessimistic lock (`SELECT ... FOR UPDATE`).

#### 3.2 Double-earn khi `CalculatePoints` gọi 2 lần

```csharp
// Line 651
var oldLines = _context.CustomerPointLine
    .Where(e => e.DocId == p.DocId && e.DocType == DocType && e.CustomerId == p.CardId)
    .ToList();
if (oldLines.Any())
    _context.CustomerPointLine.RemoveRange(oldLines);
```

Nếu user edit đơn → `CalculatePoints` gọi lại → xoá lines cũ + add lines mới. **Nhưng `CustomerPointCycle.EarnedPoint` đã được + ở `CalculatePointsCircle` không được rollback**. Khi `CalculatePointsCircle` gọi lại với edit đơn, sẽ + thêm 1 lần nữa → **double-earn**.

Không có check idempotency qua key `(DocId, OperationType)`.

#### 3.3 Hoàn điểm sai khi hoàn nhiều cycle (FIFO không reverse đúng)

Khi đổi quà với 200 điểm spread qua 3 cycle (FIFO): cycle1=80, cycle2=80, cycle3=40.
Khi huỷ đổi quà (`RedeemPoints(p, "C")`):

```csharp
var historyGrouped = historyList
    .GroupBy(h => h.PoitnSetupId)
    .ToDictionary(g => g.Key, g => g.Sum(x => x.PointChange));

foreach (var kv in historyGrouped)
{
    var cycle = cycles.FirstOrDefault(c => c.PoitnSetupId == setupId);
    // ⚠ FirstOrDefault = chỉ lấy 1 cycle
    if (cycle != null)
    {
        cycle.RedeemedPoint -= usedPoint;  // toàn bộ usedPoint dồn về 1 cycle
        cycle.RemainingPoint += usedPoint;
    }
}
```

Nếu 1 setup có nhiều cycle (vd cycle 2024 + cycle 2025): tất cả refund dồn vào cycle đầu tiên thay vì phân bổ ngược theo FIFO ban đầu → cycle balance sai vĩnh viễn.

#### 3.4 Catch nuốt exception trong `CalculatePointCheck`

```csharp
catch (Exception ex)
{
    return null;  // ← null trả về client → NullReferenceException downstream
}
```

Không log, không throw, không trace. User thấy lỗi "vô thức".

#### 3.5 Logic match item sai trong `CalculatePoints` so với `CalculatePointCheck`

So sánh 2 method, cùng input → có thể trả số điểm KHÁC NHAU:

**CalculatePointCheck** (line 503-521):
```csharp
if (line.ItemType.Where(e => e.ItemType == "I").Any(...)) {
    if (matchIndustry && matchBrand) totalPoints += ...;
}
else {
    bool matchItemType = !line.ItemType.Where(e => e.ItemType == "I").Any()
        && line.ItemType.Where(e => e.ItemType == "G").Any(...);
    bool matchPacking = !line.Packings.Any() || ...;
    if (matchIndustry && matchBrand && matchItemType && matchPacking) ...
}
```

**CalculatePoints** (line 613-627):
```csharp
bool matchPacking = !rule.Packings.Any() || rule.Packings.Any(...);  // luôn check Packing
if (matchIndustry && matchBrand && matchPacking && (matchItemDirect || matchItemGroup))
    point = rule.Point * dl.Quantity;
```

→ Ở `CalculatePointCheck`, khi match Item-direct (`ItemType == "I"`), **không check Packing**.
→ Ở `CalculatePoints`, **luôn check Packing**.

⇒ User preview thấy 100 điểm, đến lúc lưu thực tế lại được 0 điểm. **Bug nghiêm trọng về mặt UX và có thể dẫn đến tranh chấp pháp lý** (cam kết tích điểm khác với thực nhận).

#### 3.6 `CalculatePointsCircle` — block else nhánh sai logic

```csharp
else if (string.IsNullOrEmpty(Status) && Type == "C")
{
    var docRef = _context.ODOC.FirstOrDefault(e => e.Id == DocId && (e.Status == "DXN" || e.Status == "DGH"));
    if (docRef != null)
    {
        // trừ điểm
    }
    // ⚠ tạo reverseDetails BẤT KỂ docRef có null hay không
    var reverseDetails = details.Where(d => d.PointChange > 0)...
}
```

Khi huỷ đơn `Status != DXN/DGH` (vd đơn đã DTT hoàn thành): không trừ điểm cycle nhưng vẫn ghi reverse history → balance lệch.

#### 3.7 `DateTime.Now` vs `DateTime.UtcNow` mix

- Line 482: `e.FromDate.Date <= DateTime.Now.Date` (local time)
- Line 749: `DocDate = DateTime.UtcNow` (UTC)
- Line 870: `DateTime DocDate = DateTime.Now;`
- Line 942: `DocDate = DateTime.Now`

Khi server timezone Asia/Ho_Chi_Minh (+07:00) và setup `FromDate = 2026-01-01` UTC, đơn lúc 23:30 ngày 2025-12-31 UTC sẽ thấy `DateTime.Now.Date = 2026-01-01` → match setup chưa active → tích điểm sai.

**Hậu quả**: bug lệch ngày ranh giới chương trình tích điểm.

#### 3.8 `CalculatePointsCircle` — `cycle.RemainingPoint -= sum` âm khi sum > RemainingPoint

```csharp
cycle.RemainingPoint -= sum;  // không clamp, có thể âm
cycle.EarnedPoint -= sum;
```

Với edge case (huỷ đơn sau khi customer đã đổi quà ăn vào điểm đó): RemainingPoint có thể âm. Cần policy: clamp về 0, hoặc throw, hoặc tạo "negative cycle" làm placeholder.

### 🟠 HIGH — anti-pattern + scalability

#### 3.9 N+1 query trong `CalculatePoints`

```csharp
var validSetups = _context.PointSetups...ToList();   // load all active setups
foreach (var setup in setups)
    foreach (var rule in setup.PointSetupLine)
        foreach (var dl in docLines)
            // O(setups × rules × items) match in-memory
```

Nếu có 50 setup × 10 rule × 20 item = 10000 lần loop. Match logic chạy trong app, không tận dụng index DB.

#### 3.10 Không transaction

Tất cả method (`CalculatePoints`, `CalculatePointsCircle`, `RedeemPoints`) gọi `SaveChangesAsync` mà không bao quanh `BeginTransaction`. Nếu exception giữa chừng (vd save đợt 1 thành công, đợt 2 fail) → state không nhất quán giữa `CustomerPoint` và `CustomerPointCycle`.

#### 3.11 `_context.Item.Where(...).ToList()` tải toàn bộ entity Item chỉ để lấy 5 field

```csharp
var items = _context.Item.Where(i => itemIds.Contains(i.Id)).ToList();
```

Item entity lớn (có thể 50+ cột). Nên `.Select(new { Id, ItemCode, IndustryId, ... })` để giảm IO.

#### 3.12 `MapToViewDto` và `MapToViewData` duplicate ~50 dòng logic

2 DTO khác nhau (View**Dto** vs View**Data**) nhưng map gần giống nhau. Magic — chỉ khác `IndustryIds` (list ints) vs `Industries` (list ViewDto objects). Tách method base + 2 wrappers.

#### 3.13 `MapToViewData` không null-safe

```csharp
Lines = entity.PointSetupLine.Select(l => new PointSetupLineViewData {...})
```

Nếu `PointSetupLine` null → NullReferenceException. Code gọi `MapToViewData` ở `GetByIdAsync` luôn có Include nên OK, nhưng `MapToViewData` được public → external code có thể truyền entity chưa load Include.

#### 3.14 Magic strings rải rác

- `"C"`, `"G"`, `"All"` (loại customer point setup)
- `"I"`, `"G"` (item-direct vs item-group)
- `"CP"`, `"C"` (action type — Create-Pending? Cancel?)
- `"DXN"`, `"DGH"`, `"DHT"`, `"DTT"`, `"CXN"` (doc status)
- `22`, `12`, `1250000001` (ObjType)

→ Không có enum. Đổi giá trị 1 chỗ phải sửa N nơi. Mục đích "C" trong `RedeemPoints("C")` khác "C" trong `PointSetupCustomer.Type = "C"` → confusing.

### 🟡 MEDIUM — testability

#### 3.15 God class 1168 dòng, không thể test isolated

Method `CalculatePoints` đơn lẻ:
- Phụ thuộc `_context.Item`, `_context.PointSetups`, `_context.BP`, `_context.CustomerPointLine`, `_context.CustomerPoint`
- Hardcode `DateTime.Now`
- Trả `Task` (void) → không assert được output
- Không có interface inject được cho match logic

Test path: chỉ qua integration test với InMemory DB → chậm + không cover edge case match logic.

#### 3.16 `RedeemPoints` mix 2 luồng khác nhau qua param `Type`

```csharp
public async Task<Mess> RedeemPoints(CalculatorPoint p, string Type)
{
    if (Type.Equals("CP")) { /* tạo redemption */ }
    else if (Type.Equals("C")) { /* huỷ redemption */ }
}
```

Nên tách thành 2 method `RedeemAsync` và `CancelRedeemAsync`. Tương tự `CalculatePointsCircle` mix 3 nhánh (`CP`, `C`, default).

#### 3.17 Interface `IPointSetupService` rò rỉ implementation detail

```csharp
Task CalculatePointsCircle(int DocId, int CardId, string Type, string? Status);
```

`Type` và `Status` là string không enum. Caller phải biết "CP" với "C" và Status null/"D" có nghĩa gì. Không có XML doc.

## 4. Đề xuất kiến trúc mới

### 4.1 Tách responsibility theo CQRS-light

```
Service/Promotions/
├── PointSetup/                       # Configuration CRUD (chỉ admin)
│   ├── IPointSetupService.cs
│   └── PointSetupService.cs          # Create/Update/Get/List
│
├── PointCalculation/                 # Pure business logic, không phụ thuộc EF
│   ├── IPointRuleMatcher.cs
│   ├── PointRuleMatcher.cs           # Match item ↔ rule (testable 100%)
│   └── PointCalculator.cs            # Tính tổng điểm cho 1 đơn
│
├── PointTransaction/                 # State management — write side
│   ├── IPointTransactionService.cs
│   ├── PointTransactionService.cs    # Apply transaction với optimistic lock
│   └── Events/
│       ├── EarnPointsEvent.cs        # Đặt đơn → tích điểm pending
│       ├── ConfirmEarnEvent.cs       # Đóng đơn → confirm điểm
│       ├── CancelEarnEvent.cs        # Huỷ đơn → rollback
│       ├── RedeemPointsEvent.cs      # Đổi quà
│       └── CancelRedeemEvent.cs      # Huỷ đổi quà
│
├── PointReporting/                   # Read-side, optimized queries
│   ├── IPointReportService.cs
│   └── PointReportService.cs
│
└── PointBalance/                     # Helper: get current balance
    └── IPointBalanceProvider.cs      # Sum from CustomerPointCycle với cache
```

### 4.2 Pure functions cho rule matching

```csharp
public interface IPointRuleMatcher
{
    /// <summary>
    /// Pure: input deterministic → output deterministic. Không phụ thuộc DB/time.
    /// </summary>
    bool Matches(PointSetupLine rule, DocLine docLine);

    /// <summary>Tính điểm khớp 1 line — null nếu không match.</summary>
    double? CalculatePoints(PointSetupLine rule, DocLine docLine);
}
```

Test coverage 100% qua unit test với mock data:
```csharp
[Theory]
[InlineData("I", "ITEM001", true, true, 100)]  // item direct match → 100 điểm
[InlineData("G", "ITEM001", false, true, 0)]   // group match nhưng packing fail → 0
public void Matches_ScenarioBased(string itemType, ...)
```

### 4.3 Idempotency + optimistic locking

```csharp
public class CustomerPointCycle
{
    // ... existing fields
    [Timestamp] public byte[] RowVersion { get; set; }   // ← thêm
}

public class CustomerPoint
{
    // ... existing fields
    [Required, MaxLength(100)]
    public string IdempotencyKey { get; set; }   // (DocId, OperationType, Sequence)
}

// Unique index để DB block duplicate operation
[Index(nameof(IdempotencyKey), IsUnique = true)]
```

Khi apply transaction:
1. Insert `CustomerPoint` với `IdempotencyKey = $"{DocId}:Earn"` — UPSERT semantics, duplicate sẽ throw `DbUpdateException`
2. Update `CustomerPointCycle` với optimistic check qua `RowVersion`
3. Nếu conflict → retry với data mới (Polly + RetryPolicy)
4. Tất cả trong 1 `BeginTransaction`

### 4.4 Event-sourcing nhẹ

```csharp
public abstract record PointEvent(int CustomerId, DateTime OccurredAt, string IdempotencyKey);
public record EarnPointsEvent(int CustomerId, int DocId, IList<DocLineEarn> Lines, DateTime OccurredAt, string IdempotencyKey)
    : PointEvent(CustomerId, OccurredAt, IdempotencyKey);
// ... CancelEarn, Redeem, CancelRedeem

public interface IPointTransactionService
{
    Task<TransactionResult> ApplyAsync(PointEvent evt, CancellationToken ct);
}
```

Lợi:
- 1 method `ApplyAsync` thay vì 4 method (`CalculatePoints`, `CalculatePointsCircle`, `RedeemPoints`, etc.)
- Test với in-memory state mocked
- Audit log: mọi event lưu vào table mới `PointEventLog` → trace được state qua replay
- Rollback bug dễ: chèn `CompensatingEvent` thay vì sửa state trực tiếp

### 4.5 Enum hoá magic strings

```csharp
public enum PointSetupCustomerType { Specific, Group, AllCustomers }
public enum ItemMatchType { ItemDirect, ItemGroup }
public enum DocStatus { DXN, DGH, DHT, DTT, CXN, CTT, DXL }
public enum DocObjType { Order = 22, VPKM = 12, Issue = 1250000001 }
```

Enum convert ↔ string ở mapping layer; business logic chỉ thấy enum.

### 4.6 DateTime injection

```csharp
public interface ISystemClock { DateTime UtcNow { get; } }
public class SystemClock : ISystemClock { public DateTime UtcNow => DateTime.UtcNow; }

public class PointCalculator
{
    private readonly ISystemClock _clock;
    public PointCalculator(ISystemClock clock) { _clock = clock; }

    public bool IsSetupActive(PointSetup s)
        => s.IsActive && s.FromDate <= _clock.UtcNow && _clock.UtcNow <= (s.ExtendedToDate ?? s.ToDate);
}
```

Test:
```csharp
var fakeClock = new FakeClock(new DateTime(2026, 6, 1));
var sut = new PointCalculator(fakeClock);
// Assert behavior tại thời điểm cụ thể
```

## 5. Plan triển khai (8 tuần, 1 PR/tuần)

| Week | Việc | Priority | Risk |
|---|---|---|---|
| 1 | Add `RowVersion` + `IdempotencyKey` migration. Add `ISystemClock` DI | 🔴 | Low |
| 2 | Extract `IPointRuleMatcher` + 30+ unit tests đảm bảo logic identical với `CalculatePoints` hiện tại | 🔴 | Low |
| 3 | **Fix bug 3.5**: thống nhất logic match giữa `CalculatePointCheck` và `CalculatePoints` | 🔴 | Medium — UX impact, cần stakeholder approve |
| 4 | Wrap `CalculatePoints` + `CalculatePointsCircle` trong `BeginTransaction`. Thêm Polly retry cho concurrency conflict | 🔴 | Medium |
| 5 | Thay `DateTime.Now` → `_clock.UtcNow`. Thêm `IsoDateOnly` helper cho compare ngày | 🟠 | Low |
| 6 | **Fix bug 3.3**: hoàn điểm theo từng cycle thay vì dồn về cycle đầu | 🔴 | High — cần data migration cho records lỗi cũ |
| 7 | Refactor `RedeemPoints(p, Type)` → `RedeemAsync` + `CancelRedeemAsync` riêng | 🟠 | Low |
| 8 | Enum hoá magic strings, update mọi caller. Cleanup commented code (line 314-446 dead code) | 🟡 | Medium |

## 6. Test strategy

### 6.1 Unit tests (priority)

```csharp
public class PointRuleMatcherTests
{
    // Cover toàn bộ matrix: industry × brand × packing × item-direct × item-group
    [Theory] [MemberData(nameof(MatchScenarios))]
    public void Matches_AllScenarios(...) { ... }
}

public class PointCalculatorTests
{
    [Fact] public void EarnPoints_NoSetupActive_ReturnsZero() { }
    [Fact] public void EarnPoints_AllCustomerSetup_AppliesEvenIfNotInList() { }
    [Fact] public void EarnPoints_MultipleSetups_PicksMostSpecific() { }
}

public class PointTransactionServiceTests
{
    [Fact] public async Task EarnPoints_DuplicateIdempotencyKey_Throws() { }
    [Fact] public async Task RedeemPoints_ConcurrentRequests_OnlyOneSucceeds() { }
    [Fact] public async Task CancelRedeem_RestoresOriginalCycleAllocation() { }
}
```

### 6.2 Integration tests

```csharp
public class PointFlowEndToEndTests : IntegrationTestBase
{
    [Fact] public async Task FullFlow_EarnConfirmRedeemCancelRefund_BalanceCorrect()
    {
        // Setup: create PointSetup, customer, items
        await CreateOrder(customerId, items); // tích điểm pending
        await ConfirmOrder(orderId);          // EarnedPoint += 100
        await RedeemReward(customerId, 80);   // Redeemed += 80, Remaining = 20
        await CancelRedeem(redemptionId);     // Refund 80 — Remaining = 100
        await CancelOrder(orderId);           // Earned -= 100, Remaining = 0

        var balance = await GetBalance(customerId);
        Assert.Equal(0, balance.Remaining);
    }
}
```

### 6.3 Property-based tests

```csharp
[FsCheckProperty]
public void RemainingPoint_NeverNegative_AfterAnySequenceOfTransactions(
    NonEmptyArray<PointTransaction> transactions)
{
    var state = ApplyAll(transactions);
    Assert.True(state.Cycles.All(c => c.RemainingPoint >= 0));
}
```

## 7. Câu hỏi cho stakeholder business

1. **Bug 3.5 (logic match khác giữa preview và lưu)** — fix theo cách nào?
   - Option A: ưu tiên packing check (như `CalculatePoints`) → user nhận ít điểm hơn dự kiến nhưng đúng business intent
   - Option B: bỏ packing check khi item-direct (như `CalculatePointCheck`) → user nhận đúng như preview, có thể lệch business intent

2. **Bug 3.3 (hoàn điểm sai cycle)** — có cần migrate data lịch sử không? Hiện cycle có sai trên prod không (chưa kiểm tra)?

3. **Bug 3.7 (DateTime.Now vs UtcNow)** — đơn ranh giới ngày trước đây nếu lệch thì xử lý thế nào? Compensate điểm cho khách hàng?

4. **Concurrency**: hệ thống có chấp nhận user nhận lỗi 409 Conflict khi đổi quà concurrent (cần retry) không? Hay phải đảm bảo UX silent retry?

## 8. Quick wins có thể làm ngay (< 1 ngày)

Nếu chưa muốn refactor lớn, fix các vấn đề sau ngay:

1. **Bug 3.4** (catch nuốt exception): thêm `_logger.LogError(ex, "...")` rồi `throw` — 1 file, 5 phút
2. **Bug 3.7** (DateTime.Now → UtcNow): replace toàn file — 10 phút
3. **Dead code** (line 314-446 commented out): xoá — 1 phút
4. **Bug 3.6** (else nhánh sai): thêm check `if (docRef != null)` cho cả block reverseDetails — 5 phút
5. **Bug 3.4** (catch trong `RedeemPoints` line 1015 nuốt exception trả Mess thay vì throw → controller swallow): refactor sang throw `BusinessException`, middleware xử lý — 30 phút

Tôi có thể implement quick wins ngay nếu bạn approve. Refactor lớn cần discussion + multiple PRs.

## 9. References

- Code hiện tại: [Service/Promotions/PointSetupService.cs](../BackEndAPI/Service/Promotions/PointSetupService%20.cs) (lưu ý dấu cách trong tên file — bug nhỏ)
- Caller sites: `DocumentService:1783,1814`, `PvService:376,472,657,755`, `BusinessPartnerService:2005,2042,2051`, `PointSetupController:83`
- Related entity: [Models/Promotion/PointSetup.cs](../BackEndAPI/Models/Promotion/PointSetup.cs), `CustomerPoint.cs`, `ExchangePoint.cs`
