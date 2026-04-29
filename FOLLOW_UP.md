# Follow-up items

Ghi nhận trong quá trình branch `refactor/security-fixes-backend` (2026-04-29).
Các vấn đề ngoài phạm vi prompt hoặc cần làm rõ intent trước khi sửa.

## 1. `[AllowAnonymous]` không kèm `[PrivilegeRequirement]` — cần xác minh intent

Các endpoint dưới đây có `[AllowAnonymous]` đứng một mình (không bị đè bởi
`[PrivilegeRequirement]` nên không phải bypass theo nghĩa C-3). Tuy nhiên
vì các thao tác liên quan tới dữ liệu nội bộ, cần user xác nhận đây là
webhook/cron có chủ đích hay sót bảo mật:

- `Controllers/CustomerController.cs`
  - `POST /api/customer/sync`        (line ~76-79)
  - `POST /api/customer/syncbp`      (line ~107-110)
  - `POST /api/customer/syncCrd4`    (line ~121-124)
  - `POST /api/customer/syncCrd4CardCode` (line ~130-133)
  - `POST /api/customer/syncissue`   (line ~138-141)

- `Controllers/ApprovalController.cs`
  - `GET  /api/approval/{id:int}`              (line 37-38)
  - `POST /api/approval/action-purchase/{id:int}` (line 72-73)

Nếu đây là webhook từ SAP/cron job, đề xuất verify bằng signature hoặc IP
allowlist thay vì để anonymous.

## 2. `PayOne._config` static field — H-10 chưa fix trong session này

`Program.cs:112-116` đổ `PayoneConfig` vào static field `PayOne._config`.
Refactor sang `IOptions<PayOneConfig>` qua DI cần đụng tất cả các nơi
consume `PayOne._config` → out-of-scope cho prompt security đơn lẻ.

## 3. C-1 hardcoded secrets — đã verify, chờ user quyết định hướng xử lý

Xem mục "Lưu ý cho lần sau" trong `CHANGELOG_REFACTOR.md`.

## 4. `BadRequest(ModelState)` ở 3 controller (INTEGRATION_REVIEW I-4.1)

ApprovalTemplateController, PaymentController, CustomerController còn dùng
`return BadRequest(ModelState)` thay vì wrap thành `ApiResponse.Fail` với
errors phẳng. Out of scope cho prompt security; nằm trong nhóm cải thiện
error format INTEGRATION_REVIEW I-4.1.

## 5. Async — `Approval.cs:CreateApproval` dual-usage method ✅ DONE

Đã giải quyết trong branch `refactor/async-await-fixes` (2026-04-29) qua Option C:

- Verify `grep "_eventAggregator.Publish.*Approval"` → 0 publishers cho
  `Models.Approval.Approval` → subscription là dead code.
- Xoá `eventAggregator.Subscribe<Models.Approval.Approval>(CreateApproval)` ở
  constructor.
- Đổi `private void CreateApproval` → `private async Task CreateApprovalAsync`,
  chuyển transaction + SaveChanges + Rollback sang async overload.
- Sửa caller `ActionApproval` line 59: `CreateApproval(app)` →
  `await CreateApprovalAsync(app)`.
- Không đổi public API.

## 6. Async — CancellationToken propagation toàn hệ thống

User yêu cầu "Method async không nhận CancellationToken ở chỗ cần (controller,
long-running task) — thêm vào và truyền xuyên suốt".

Sample audit cho thấy CT đã có ở một số nơi:
- `BPSyncService.SyncCRD4DeltaAsync(CancellationToken ct)` ✓
- Các Quartz job nhận `IJobExecutionContext` (có CT bên trong) ✓

Phần lớn còn lại CHƯA có CT:
- ~75 controller, hầu hết action method không nhận `CancellationToken` parameter.
- Service layer (~47 service) không truyền CT xuống EF Core (`ToListAsync()`,
  `SaveChangesAsync()` không có ct argument).

Để propagate CT đầy đủ cần:
1. Thêm `CancellationToken ct` vào mọi controller action (ASP.NET Core tự bind
   `HttpContext.RequestAborted` vào tham số này).
2. Thêm `CancellationToken ct = default` vào mọi method async của service.
3. Truyền `ct` xuống EF (`ToListAsync(ct)`, `SaveChangesAsync(ct)`,
   `FirstOrDefaultAsync(predicate, ct)`).

Quy mô: ước tính 200-400 method cần đụng. Effort 16-24h tập trung. Không phù
hợp nhồi vào prompt async fix lẻ.

Đề xuất: tạo branch riêng `refactor/cancellation-token-propagation`, chia làm
3 sprint con (controllers, top services, EF leaf). Hoặc bắt đầu enforce qua
analyzer rule `CA2016` (Forward CancellationToken) cho code mới.

## 8. EF perf — H-5 ApprovalWorkFlow N+1 ✅ DONE

Đã giải quyết trong branch `refactor/approval-engine-batch` (2026-04-29).

- Thêm `Task<Dictionary<int, object?>> GetEntitiesAsync(IReadOnlyCollection<int>)`
  vào `IApprovalWorkFlowEngine` + abstract trong `BaseWorkFlowEngineService`.
- 3 engine (`PurchaseOrderWorkFlow`, `RequestPickUpItemsWorkFlow`,
  `PurchaseReturnWorkFlow`) override với single `WHERE Id IN (...)` query
  trên `context.ODOC` + `AsNoTracking()`.
- `ApprovalWorkFlowService.GetAllAsync` group rows theo `DocumentType`, gọi
  batch 1 lần per group → N+1 → ~1-3 query (theo số DocumentType khác nhau
  trong page).
- **Public API change**: interface mở rộng (thêm method mới) — caller bên
  ngoài implement `IApprovalWorkFlowEngine` (nếu có) phải implement thêm
  method này. Trong codebase chỉ 3 implementation đều đã update.

## 9. EF perf — đề xuất thêm index để hỗ trợ filter/join hot path

Quan sát từ các query đã sửa trong commit này. Dưới đây là các column được
WHERE/JOIN nhiều lần. Kiểm tra xem có index chưa (qua SSMS hoặc
`AppDbContext.OnModelCreating`); nếu chưa, đề xuất EF migration:

| Bảng | Column | Lý do |
|---|---|---|
| `Item` | `ItemCode` | DocumentService load Item by `ItemCode IN (...)` rất nhiều chỗ. Nếu chưa unique index hoặc nonclustered index trên cột này → seek thành scan. |
| `WTM2` | `Sort` + `OWTM_Id` (FK) | Approval.cs:106-108 filter `Sort == 1 && OWTM.Active`. FK trên OWTM_Id thường có index, nhưng composite (OWTM_Id, Sort) sẽ khớp predicate tốt hơn. |
| `OWTM` | `Active` | Filter chính của approval lookup. Nếu phần lớn rows Active=true thì index ít hữu ích, ngược lại rất hữu ích. |
| `RUsers` (m2m) | `(OWTM_Id, AppUser_Id)` | Pivot table — nếu chưa có composite index thì `RUsers.Any(p => p.Id == X)` sẽ scan. Thường EF tự tạo PK composite ở pivot, kiểm tra lại. |
| `ItemSpec` | `IndustryId`, `BrandId`, `ItemTypeId` | ItemService.cs:1215 filter trên 3 cột này. Composite index theo selectivity (IndustryId thường nhất) sẽ tăng hiệu quả. |
| `Promotion` | `(FromDate, ToDate, PromotionStatus)` | PromotionService.cs:172-173 filter date-range + status, gọi rất nhiều. |

**KHÔNG tự generate migration trong commit này** vì:
- Cần verify schema hiện tại trước (có thể đã có index do EF convention).
- Latest migration `Add_RefreshToken` (Apr 2026); thêm migration mới phải đồng
  bộ với DBA về plan apply.
- Một số index đề xuất đụng bảng tải lớn (`Item`, `Promotion`) — cần
  `WITH ONLINE = ON` hoặc maintenance window.

Đề xuất user:
1. Confirm bảng nào cần index → tôi generate migration `dotnet ef migrations
   add Add_PerfIndexes_*` ở branch riêng `chore/perf-indexes`.
2. Hoặc chạy thủ công `CREATE NONCLUSTERED INDEX ... WITH (ONLINE = ON)` qua
   SSMS rồi scaffold migration sau.

## 10. EF perf — benchmark trước khi merge cho 2 hotpath

Hai chỗ đáng benchmark trước khi merge `refactor/ef-query-optimization`:

1. **`DocumentService.SyncToSapAsync` (line ~528, ~8168)** — benchmark trước/sau:
   - **Setup:** doc với ~5 promotion line, Item table có ~50k rows.
   - **Trước:** `_context.Item.ToList()` → load 50k rows.
   - **Sau:** filter `WHERE ItemCode IN (5 codes)` → load ~5 rows.
   - **Đo:** thời gian gọi `SyncToSap`, peak memory, # SQL query.
   - **Kỳ vọng:** thời gian giảm 80%+, memory giảm tương ứng.

2. **`Approval.CreateApprovalAsync` (line ~101)** — benchmark trước/sau:
   - **Setup:** WTM2 có ~1000 rows active, RUsers per OWTM ~10 user.
   - **Trước:** load all 1000 WTM2 + lazy nav → in-memory filter.
   - **Sau:** SQL `WHERE EXISTS (SELECT 1 FROM RUsers WHERE Id = @actorId)`.
   - **Đo:** thời gian, # rows transferred.
   - **Kỳ vọng:** từ 1000 rows → ~1 row được trả về, 50%+ thời gian giảm.

Test framework: `BenchmarkDotNet` hoặc đơn giản hơn — `Stopwatch` + `dotnet test`
với fixture chứa SQL Server seeded data. Nếu không tiện cài Bench, có thể chạy
SSMS với `SET STATISTICS TIME, IO ON` cho 2 query thô và so sánh.

## 11. Async — sync EF call (`FirstOrDefault`, `ToList`...) trong method async (was #7)

Khi sửa C-4 ở `AccountService.AuthenticateUser` phát hiện:
```csharp
var user = _context.AppUser.AsSplitQuery()...FirstOrDefault(...);   // sync trong async
```
Tương tự, Approval.cs:47, Approval.cs:106, và nhiều chỗ khác trong DocumentService
vẫn dùng `.FirstOrDefault()`, `.ToList()`, `.Count()` đồng bộ trong async method.

Đây là cùng họ vấn đề H-11 (sync EF trong async) nhưng không được liệt kê
riêng trong BACKEND_REVIEW. Out of scope cho prompt async-await đơn lẻ vì
quy mô lớn (rải rác toàn codebase, dễ nhầm với LINQ-to-objects). Đề xuất
gắn analyzer rule `EF1001`/CA1849 hoặc audit theo hot path.
