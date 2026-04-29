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
