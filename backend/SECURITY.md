# Security — APSP NEW_BACKEND

## 🚨 INCIDENT: SQL password đã leak qua log lịch sử

**Phát hiện 2026-04-28** trong commit `110240a`: trước đó `Program.cs:135` có
dòng `Console.WriteLine(connectionString);` in **decrypted** SQL connection
string (kèm password `Foxai@2024`) ra stdout mỗi lần app khởi động.

**Hệ quả**: mọi log container/syslog/cloud logging từ deploy đầu tiên đến
2026-04-28 đều chứa SQL password trong plain text.

**Hành động bắt buộc** (trước khi xử lý các leak khác bên dưới):

1. **Rotate SQL `sa` password ngay** — coi như public knowledge
2. **Audit log lịch sử** ở mọi nơi log có thể được lưu:
   - Container stdout (Docker logs / K8s logs / journalctl)
   - Cloud Watch / Stackdriver / Splunk / Datadog
   - File `BackEndAPI/Logs/log-*.txt` (Serilog rolling files)
3. **Purge log cũ** chứa password (truncate/delete files, request log
   provider xoá)
4. Kiểm tra ai có truy cập log đó — assume password đã leak ra ngoài

Sau commit `110240a`, dòng `Console.WriteLine` đã được xoá → log mới sẽ sạch.

---

## ⚠️ ROTATE: secrets đã public trên GitHub

Commit `0463b31` (init project) push lên `main` chứa secrets thật:

| Secret | File | Action |
|---|---|---|
| SQL `sa` password (`Foxai@2024`) | `appsettings.json` line 4 (encrypted nhưng key decrypt ở `EncryptDecrypt.cs` cũng public) | Đổi password SQL Server, tạo user `apsp_app` riêng (least privilege) — xem [docs/db/create_app_user.sql](docs/db/create_app_user.sql) |
| JWT signing key | `appsettings.json` line 27 | Generate key 256-bit mới, replace; mọi JWT token cũ sẽ invalid (force user login lại) |
| SMTP password (`N0re@PSP2468`) | `appsettings.json` line 46 | Đổi password Office365 cho noreply@apsaigonpetro |
| OnePay HashCode (test creds) | line 63, 68 | Test creds, ít rủi ro nhưng nên rotate |
| VNPay HashSecret | line 79 | Liên hệ VNPay rotate merchant secret |
| Zalo RefreshToken / SecretKey | line 20, 22 | Rotate qua Zalo Business OA |
| SAP Service Layer creds | line 54-56 (encrypted) | Đổi password user SAP nếu encryption key cũng leak |

### Kế hoạch rotate đề xuất

```
T0       : Rotate SQL sa password TRƯỚC (đã leak qua log) — đây là cấp bách nhất
T0 + 5m  : Tạo secrets mới còn lại (JWT / SAP / SMTP / payment gateway)
T0 + 10m : Update env var / User Secrets ở từng môi trường
T0 + 15m : Deploy lại app — verify health check OK
T0 + 30m : Disable/revoke secret cũ (DB cũ password, JWT key cũ, ...)
T0 + 1h  : Audit + purge log lịch sử chứa SQL password cũ
T0 + 2h  : Rewrite git history nếu cần ẩn secret cũ:
           git filter-repo --path BackEndAPI/appsettings.json --invert-paths --force
           git push --force-with-lease origin main
           Lưu ý: --force-with-lease phá lịch sử cho mọi clone — phải báo team trước.
```

## Cách load secrets sau khi rotate

### Development (máy dev)

```bash
cd BackEndAPI
dotnet user-secrets set "Jwt:Key" "<base64-encrypted-key>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<encrypted-conn-str>"
dotnet user-secrets set "SmtpSettings:Password" "<smtp-password>"
# ... tất cả các key trong appsettings.example.json
```

User Secrets lưu ở `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`, không vào git.

### Staging / Production

Set qua env var (.NET Configuration tự bind dấu `__` → `:`):

```bash
export Jwt__Key="<encrypted-key>"
export ConnectionStrings__DefaultConnection="<encrypted-conn-str>"
export SmtpSettings__Password="<smtp-password>"
export SAPServiceLayer__Password="<encrypted-sap-password>"
# ...
```

Hoặc dùng:
- **Azure App Service**: Configuration → Application settings
- **AWS ECS**: Task definition → secrets từ Secrets Manager
- **Kubernetes**: Secret + envFrom

## Bảng kiểm trước khi deploy production

- [ ] `appsettings.json` không có trong git (kiểm: `git ls-files | grep appsettings.json`)
- [ ] Mọi `__PLACEHOLDER__` trong `appsettings.example.json` đã có giá trị thật ở User Secrets / env var
- [ ] `Jwt:Issuer` và `Jwt:Audience` không chứa "localhost"
- [ ] `Jwt:Key` decrypted ≥ 32 byte (app sẽ throw nếu không đủ)
- [ ] `Cors:AllowedOrigins` chỉ list domain production (app throw nếu rỗng ở non-Dev)
- [ ] `ASPNETCORE_ENVIRONMENT=Production` (kiểm tra log không có "TLS_BYPASS_ENABLED" warning)
- [ ] `ALLOW_SELF_SIGNED_TLS` chưa set (hoặc = `false`)
- [ ] User SQL không phải `sa` — dùng user riêng có quyền `db_owner` của database `APSP`

## Những thứ không cần secret nhưng phải checkin

- `appsettings.example.json` — template với placeholder
- `appsettings.Development.json` — chỉ Serilog config, không secret

## Liên hệ

Phát hiện secret trong code? **KHÔNG** mở public issue. Email trực tiếp owner repo.
