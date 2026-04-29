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

## 5. Async — `Approval.cs:CreateApproval` dual-usage method

Ghi nhận trong branch `refactor/async-await-fixes` (2026-04-29).

`Service/Approval/Approval.cs:74` định nghĩa `private void CreateApproval(...)`
được dùng theo HAI cách:

1. **Event handler** — đăng ký `eventAggregator.Subscribe<...>(CreateApproval)`
   ở constructor (line 22). `IEventAggregator.Subscribe` chỉ nhận
   `Action<TEvent>` → handler buộc phải `void`.
2. **Direct sync call** — `ActionApproval` (line 28) gọi `CreateApproval(app)`
   ở line 59, sau đó dùng `app` đã mutate để tiếp tục flow rồi
   `await _context.SaveChangesAsync()` ở line 68.

Method còn chứa `context.SaveChanges()` đồng bộ ở line 135 (BACKEND_REVIEW H-11).

Để fix đúng cần một trong:
- **Option A:** Mở rộng `IEventAggregator` để hỗ trợ `Func<TEvent, Task>`
  → public API change của internal event pattern. Tất cả `Subscribe` callsite
  phải audit.
- **Option B:** Tách logic thành `private async Task CreateApprovalCoreAsync(...)`
  + giữ `private void CreateApproval(...)` làm wrapper sync chỉ cho event
  aggregator (gọi `.GetAwaiter().GetResult()` hoặc `Task.Run` — vẫn blocking).
- **Option C:** Bỏ subscription event-aggregator, chỉ giữ direct call (kiểm tra
  có ai publish event này không).

Cùng vấn đề ở `Service/Notification/Notification.cs:24` `CreateNotification`
trước đây — đã chuyển sang `async void` (cho phép theo rule #4 cho event handler)
trong branch hiện tại. Approval thì không dùng pattern này được vì còn caller
direct đang dựa vào side-effect đồng bộ.

Cần user quyết định option trước khi refactor.

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

## 7. Async — sync EF call (`FirstOrDefault`, `ToList`...) trong method async

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
