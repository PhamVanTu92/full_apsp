# CLAUDE.md — NEW_BACKEND

## Tổng quan dự án

Backend API cho hệ thống APSP, xây dựng trên **ASP.NET Core 8.0** với SQL Server và tích hợp SAP Business One. Đây là hệ thống B2B phức tạp hỗ trợ quản lý đơn hàng, luồng phê duyệt đa cấp, thanh toán (VNPay / OnePay / Zalo), tích điểm khách hàng và đồng bộ dữ liệu với SAP.

## Stack công nghệ

| Layer | Công nghệ |
|---|---|
| Framework | ASP.NET Core 8.0 |
| ORM | Entity Framework Core 9.0.4 + SQL Server |
| Auth | JWT Bearer + ASP.NET Core Identity |
| Real-time | SignalR + WebSocket |
| Scheduler | Quartz.NET |
| Email | MailKit / MimeKit |
| Object mapping | AutoMapper 12 |
| Filtering | Gridify 2.16 |
| SAP | B1SLayer 2.1 |
| Crypto | BouncyCastle + EncryptDecrypt lib nội bộ |

## Cấu trúc thư mục

```
BackEndAPI/
├── Controllers/     # 73 controller — REST endpoints
├── Service/         # Business logic (interface + impl)
│   ├── Interface/   # ~59 interface files (IXxxService)
│   └── Implement/   # Các service class
├── Models/          # Entity models (29 sub-domain)
├── Dtos/            # Data Transfer Objects
├── Mapping/         # AutoMapper profiles
├── Data/            # DbContext + repository
├── Migrations/      # EF Core migrations
├── Middleware/       # Exception + request handling
└── Extensions/      # Helper extensions
```

## Kiến trúc cốt lõi

- **Generic Service**: `IGenericService<T>` / `GenericService<T>` — CRUD dùng chung
- **Feature Services**: Mỗi domain có interface riêng, DI đăng ký tập trung ở `Program.cs`
- **Approval Workflow**: Hệ thống phê duyệt đa cấp (V1 và V2)
- **Background Jobs**: SyncDebJob, SyncVPKMJob, SyncBPJob … chạy theo lịch Quartz

## Quy ước đặt tên

- Class / Interface: `PascalCase` — ví dụ `DocumentService`, `IDocumentService`
- Method: `PascalCase` + hậu tố `Async` khi bất đồng bộ
- DTO: `XxxDto`, `CreateXxxDto`, `UpdateXxxDto`
- Controller route: `api/[controller]` (kebab-case tự động)
- Biến cục bộ / tham số: `camelCase`

## Quy tắc bất biến

- **KHÔNG** tắt `ValidateLifetime` cho JWT trong production — cần bật lại
- **KHÔNG** commit `appsettings.json` chứa credential thật — dùng Secret Manager hoặc env var
- **KHÔNG** load toàn bộ bảng rồi mới filter — luôn dùng `IQueryable` trước `ToListAsync()`
- **LUÔN** trả về `IActionResult` có status code chuẩn
- **LUÔN** dùng `async/await` — không dùng `.Result` hay `.Wait()`
- **LUÔN** tạo DTO tách biệt với Entity — không expose Entity trực tiếp

## Quản lý secrets

`appsettings.json` đã được liệt kê trong `backend/.gitignore` → file local
của dev không bao giờ được commit. Template phiên bản hoá là
`appsettings.example.json` (chứa placeholder `__XXX__`).

`BackEndAPI.csproj` đã khai báo sẵn `UserSecretsId`, nên User Secrets
storage có thể dùng ngay không cần cấu hình thêm.

### Local dev — dùng User Secrets

```bash
cd backend/BackEndAPI

# Khởi tạo (chỉ cần lần đầu nếu UserSecretsId chưa có; ở repo này đã có sẵn)
dotnet user-secrets init

# Set giá trị — ASP.NET Core sẽ override appsettings.json khi đọc
# IConfiguration. Dùng dấu : để chỉ section lồng nhau.
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<encrypted>"
dotnet user-secrets set "Jwt:Key"             "<base64-key>"
dotnet user-secrets set "SmtpSettings:Password" "<smtp-pwd>"
dotnet user-secrets set "ZaloTokenConfig:RefreshToken" "<zalo-token>"
dotnet user-secrets set "SAPServiceLayer:CompanyDB" "<encrypted>"
dotnet user-secrets set "SAPServiceLayer:Username"  "<encrypted>"
dotnet user-secrets set "SAPServiceLayer:Password"  "<encrypted>"

# Xem các key đang lưu
dotnet user-secrets list

# Xoá 1 key
dotnet user-secrets remove "Jwt:Key"
```

User Secrets được lưu ở
`%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` (Windows)
hoặc `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json` (Linux/Mac)
— không bao giờ nằm trong repo.

### Staging / Production — environment variable

ASP.NET Core map env var thành config key bằng `__` (double underscore).
Ví dụ trong `docker-compose.yml` hoặc systemd service:

```bash
ConnectionStrings__DefaultConnection=...
Jwt__Key=...
SmtpSettings__Password=...
SAPServiceLayer__CompanyDB=...
```

Hoặc tốt hơn: dùng Azure Key Vault / Vault provider — nạp vào
`builder.Configuration.AddAzureKeyVault(...)` ở `Program.cs`.

### Quy tắc

- **KHÔNG** commit `appsettings.json`, `appsettings.Production.json`,
  `appsettings.Staging.json`, hay file `.env` chứa giá trị thật.
- **KHÔNG** log giá trị `IConfiguration` tự do — có thể leak secret.
- **KHÔNG** đặt secret vào tham số dòng lệnh (lưu trong shell history).
- Khi onboard dev mới: gửi secret qua kênh bảo mật (1Password, Bitwarden,
  KeePass), KHÔNG qua chat/email/Slack chung.

## Thứ tự đọc code khi onboard

1. `Program.cs` — hiểu DI và middleware pipeline
2. `Data/AppDbContext.cs` — schema tổng thể
3. `Service/Interface/IGenericService.cs` — contract chung
4. `Controllers/AccountController.cs` — auth flow
5. `Controllers/DocumentController.cs` — luồng nghiệp vụ chính
6. `Service/Implement/ApprovalWorkFlow/` — workflow engine

## Tích hợp ngoài (External Integrations)

| Hệ thống | Mô tả | Cấu hình |
|---|---|---|
| SAP B1 | Sync đơn hàng, đối tác, kho | `SAPServiceLayer` trong appsettings |
| VNPay | Cổng thanh toán | `VnpaySettings` |
| OnePay | Cổng thanh toán | `PayoneConfig` |
| Zalo | Nhắn tin + thanh toán | `ZaloTokenConfig` |
| SMTP | Email OTP, thông báo | `MailSettings` + `SmtpSettings` |
