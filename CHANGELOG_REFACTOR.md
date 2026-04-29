# Changelog refactor

## 2026-04-29 — refactor/security-fixes-backend

### File thay đổi

- `backend/BackEndAPI/Service/Approval/ApprovalTemplateService.cs` (modified)
- `backend/BackEndAPI/Service/BusinessPartners/BusinessPartnerService.cs` (modified)
- `backend/BackEndAPI/Service/Account/AccountService.cs` (modified)
- `backend/BackEndAPI/Service/Document/DocumentService.cs` (modified)
- `backend/BackEndAPI/Controllers/ApprovalTemplateController.cs` (modified)
- `backend/BackEndAPI/Controllers/PaymentController.cs` (modified)
- `FOLLOW_UP.md` (created)
- `CHANGELOG_REFACTOR.md` (created)

### Tóm tắt

5 commit `fix(security):` xử lý các vấn đề Critical/High thuộc nhóm Bảo mật
trong `BACKEND_REVIEW.md`:

1. **C-2 SQL injection** — `ExecuteSqlRaw` → `ExecuteSqlInterpolated` ở
   ApprovalTemplateService:171.
2. **H-2 File upload validation** — thêm whitelist extension + giới hạn
   20MB cho `BusinessPartnerService.AddFiles`.
3. **H-1 Random password yếu** — thay `System.Random` bằng
   `RandomNumberGenerator.GetInt32` + Fisher-Yates shuffle ở
   `AccountService.GenerateRandomPassword`.
4. **C-5 TLS bypass không có guard** — bọc
   `DangerousAcceptAnyServerCertificateValidator` trong
   `TlsBypass.IsEnabled` (chỉ Development hoặc env
   `ALLOW_SELF_SIGNED_TLS=true`) ở DocumentService.
5. **C-3 [AllowAnonymous] đè [PrivilegeRequirement]** — gỡ ở
   ApprovalTemplate (5 endpoint) và Payment (2 endpoint).

### Lý do

Các vấn đề Critical/High đã được flag trong BACKEND_REVIEW. Mỗi fix là
self-contained, không đổi public API contract (response shape giữ nguyên,
chỉ siết auth + validation).

### Kết quả kiểm tra

- `dotnet build BackEndAPI.sln`: **0 Error**, 554 warning (đều pre-existing).
- `dotnet test`: **71/71 PASS**.

### Lưu ý cho lần sau

**C-1 hardcoded secrets — đã chọn phương án A (an toàn):**

- `.gitignore` đã được verify — `git check-ignore backend/BackEndAPI/appsettings.json` trả đúng path → file local không được track.
- `BackEndAPI.csproj` đã khai báo `UserSecretsId` sẵn → User Secrets dùng được ngay.
- Đã thêm section "Quản lý secrets" vào `backend/CLAUDE.md` với workflow `dotnet user-secrets ...` cho dev và `__` env var pattern cho prod.
- **Không** chạm `appsettings.json` (giữ nguyên dev environment hoạt động).
- File local của dev vẫn chứa secret thật — dev chịu trách nhiệm migrate dần sang User Secrets nếu muốn (workflow đã có doc).

Phương án B/C (replace placeholder + rotate) bị từ chối vì secret chưa từng vào git history → không có incident phải xử lý gấp.

**Trade-off cho các fix khác đã được user confirm:**
- C-3: behavioral change — endpoints Approval Template / Payment giờ bắt buộc JWT.
- C-5: prod cần CA hợp lệ cho SAP B1 (hoặc set `ALLOW_SELF_SIGNED_TLS=true` cho staging).
- H-2: rollback transaction nếu file invalid trong batch.

**Khi điều tra C-1, phát hiện situation khác mô tả trong BACKEND_REVIEW:**

- `backend/BackEndAPI/appsettings.json` **đã được gitignore** (`backend/.gitignore` line 8).
- `git ls-files` không trả về file này → file local nhưng **không track**.
- `git log -S "N0re@PSP2468"` chỉ tìm thấy match trong `backend/SECURITY.md`
  (file documentation ghi chú cần rotate), không phải trong appsettings.json.
- Tức là **secrets không có trong git history**.

Vấn đề thực tế còn lại:

1. File local (untracked) chứa secret thật của dev. Nếu replace bằng
   placeholder sẽ break dev environment trừ khi user migrate sang User Secrets.
2. `appsettings.example.json` đã có placeholder tốt — không cần đổi.
3. `backend/SECURITY.md` documentation references password value — tốt nếu
   để rotate-tracking, nhưng có thể loại bỏ string thật, chỉ giữ link.

**3 phương án cho user lựa chọn:**

- **A (an toàn):** Không động `appsettings.json` (đã gitignore). Chỉ verify
  `.gitignore`, document workflow User Secrets trong `backend/CLAUDE.md`.
- **B (clean):** Replace giá trị trong local `appsettings.json` bằng
  placeholder, đồng thời generate script `dotnet user-secrets set ...` để
  user migrate. Có thể break dev env nếu chạy script không đầy đủ.
- **C (paranoid):** Như B, cộng thêm rotate **tất cả** secret hiện có vì
  giả định đã từng lộ. Cần phối hợp DevOps + thay đổi prod config.

**H-10 PayOne static field** — chưa fix vì refactor sang `IOptions<>` đụng
nhiều consumer của `PayOne._config`. Ghi vào FOLLOW_UP.md.

**Các vấn đề ngoài phạm vi** đã ghi vào `FOLLOW_UP.md`:
- `[AllowAnonymous]` đứng một mình ở Customer sync / Approval get-by-id —
  có thể là webhook chủ đích, cần user xác nhận.
- `BadRequest(ModelState)` raw — thuộc nhóm error format INTEGRATION_REVIEW I-4.1.
