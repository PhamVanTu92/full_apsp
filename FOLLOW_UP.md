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
