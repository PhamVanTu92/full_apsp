# ARCHITECTURE REVIEW — APSP Full Project

> **Ngày khảo sát:** 2026-04-29  
> **Trạng thái repo:** Clean — 1 commit duy nhất (`66ec99f` — merge backend + frontend)  
> **Mục đích tài liệu:** Mô tả hiện trạng, không đề xuất sửa đổi.

---

## 1. Cấu trúc thư mục tổng thể

```
FULL_PROJECT/
├── backend/                        # ASP.NET Core 8.0 Web API
│   ├── BackEndAPI/                 # Project chính (Web API)
│   │   ├── Controllers/            # 75 REST controllers
│   │   ├── Service/                # Business logic (~47 service modules)
│   │   ├── Models/                 # Domain entities (29 nhóm)
│   │   ├── Dtos/                   # Data Transfer Objects
│   │   ├── Data/                   # DbContext + EF config
│   │   ├── Migrations/             # EF Core migrations
│   │   ├── Mapping/                # AutoMapper profiles
│   │   ├── Middleware/             # Custom middleware
│   │   ├── Hubs/                   # SignalR hubs
│   │   ├── Infr/                   # Infrastructure helpers
│   │   ├── Extensions/             # DI extension methods
│   │   ├── Program.cs              # Entry point + DI registration
│   │   ├── appsettings.json        # Cấu hình (values đã mã hoá)
│   │   └── Dockerfile              # Multi-stage Docker build
│   ├── ClassLibrary/               # Project EncryptDecrypt
│   ├── Area/                       # Project Area/region models
│   ├── BackEndAPI.Tests/           # Unit test project (rỗng)
│   ├── BackEndAPI.sln
│   └── docker-compose.yml
│
└── frontend/                       # Vue 3 + Vite SPA
    ├── src/
    │   ├── api/                    # Axios instance + interceptors
    │   ├── components/             # ~50 reusable components
    │   ├── composables/            # Vue 3 composable functions
    │   ├── config/                 # App config
    │   ├── helpers/                # Utility functions
    │   ├── i18n/                   # Vue I18n setup
    │   ├── locales/                # vi.json, en.json
    │   ├── layout/                 # AppLayout, Sidebar, Topbar
    │   ├── middlewares/            # Route guards
    │   ├── Pinia/                  # Pinia stores (state chính)
    │   ├── router/                 # Vue Router (lazy-loaded)
    │   ├── services/               # Business services
    │   ├── store/                  # Vuex (legacy, chỉ còn auth)
    │   ├── utils/                  # TypeScript utilities
    │   ├── views/                  # Page components
    │   │   ├── admin/              # Màn hình quản trị
    │   │   ├── auth/               # Login, Register, OTP
    │   │   ├── client/             # Màn hình khách hàng
    │   │   └── common/             # Màn hình dùng chung
    │   ├── assets/                 # Images, CSS, SVG
    │   ├── App.vue
    │   └── main.js
    ├── vite.config.mjs
    ├── tsconfig.json
    ├── package.json
    └── .env.{development|uat|production}
```

---

## 2. Framework & Library — Phiên bản chính

### 2.1 Backend

| Package | Phiên bản | Vai trò |
|---|---|---|
| ASP.NET Core | **8.0** | Web framework (TFM: net8.0) |
| Entity Framework Core | **9.0.4** | ORM chính (SQL Server provider) |
| NHibernate | **5.5.3** | ORM legacy (vẫn còn tồn tại song song) |
| Z.EntityFramework.Extensions | **9.103.9.3** | Bulk insert/update |
| ASP.NET Core Identity | **8.0.6** | User/role management |
| JWT Bearer | **8.0.6** | Xác thực token |
| AutoMapper | **12.0.1** | Object mapping |
| Quartz.NET | **3.14.0** | Background job scheduler |
| B1SLayer | **2.1.0** | SAP Business One Service Layer client |
| MailKit / MimeKit | **4.14.0** | Email (SMTP) |
| SignalR (AspNetCore) | **1.0.4** | Real-time WebSocket |
| Serilog.AspNetCore | **8.0.x** | Structured logging |
| OpenTelemetry | **1.10.x** | Metrics + distributed tracing |
| Swashbuckle (Swagger) | **6.4.0** | API documentation |
| Gridify | **2.16.2** | Dynamic filtering / pagination |
| LinqKit | **1.3.7** | LINQ query composition |

### 2.2 Frontend

| Package | Phiên bản | Vai trò |
|---|---|---|
| Vue | **3.2.41** | UI framework (Composition API) |
| Vite | **4.2.1** | Build tool |
| TypeScript | **5.8.3** | Type-safe JS |
| Vue Router | **4.1.5** | SPA routing |
| Pinia | **2.2.2** | State management (chính thức) |
| Vuex | **4.1.0** | State management (legacy, đang loại bỏ) |
| PrimeVue | **3.50.0** | Component UI library |
| PrimeFlex | **3.3.1** | CSS utilities (PrimeVue ecosystem) |
| TailwindCSS | **3.4.3** | Utility-first CSS |
| Axios | **1.7.2** | HTTP client |
| @microsoft/signalr | **8.0.7** | WebSocket client (real-time) |
| Vue I18n | **11.1.12** | Đa ngôn ngữ (vi / en) |
| ExcelJS | **4.4.0** | Export Excel |
| jsPDF | **3.0.1** | Export PDF |
| date-fns | **3.6.0** | Xử lý ngày tháng |
| @vueuse/core | **11.0.1** | Vue composable utilities |
| CKEditor 5 | **42.0.2** | Rich text editor |
| Vitest | **4.1.5** | Unit test runner |

---

## 3. Kiến trúc tổng thể

### 3.1 Backend — Kiến trúc phân tầng (Layered Architecture)

Dự án **không** áp dụng Clean Architecture hay DDD chính thống. Cấu trúc là Layered Architecture đơn giản:

```
HTTP Request
    ↓
[Middleware Layer]
    ApiResponseFilter   — wrap tất cả response thành ApiResponse<T>
    ExceptionHandling   — bắt exception toàn cục
    JwtMiddleware       — xác thực token
    ↓
[Controller Layer]  (75 controllers)
    — Nhận request, validate input cơ bản, gọi service
    ↓
[Service Layer]     (~47 service modules)
    — Business logic, orchestration, gọi repository
    ↓
[Data Layer]
    AppDbContext        — EF Core DbContext (kế thừa IdentityDbContext)
    GenericService<T>   — CRUD tổng quát cho mọi entity
    ↓
[Database]          SQL Server
```

**Các pattern đáng chú ý:**

- **Generic Repository/Service:** `IGenericService<T>` + `GenericService<T>` — cho phép CRUD nhanh không cần viết thêm service riêng cho từng entity.
- **Approval Workflow Engine (V2):** Factory pattern — `ApprovalWorkFlowFactory` resolve đúng engine (`PurchaseOrderWorkFlow`, `RequestPickUpItemsWorkFlow`, `PurchaseReturnWorkFlow`) dựa trên loại document.
- **API Response Wrapper:** `ApiResponseFilter` tự động bọc mọi response controller vào `ApiResponse<T>` — chuẩn hoá JSON API contract.
- **Background Jobs (Quartz.NET):** SAP sync jobs (`SyncDebJob`, `SyncVPKMJob`, `SyncBPJob`) chạy theo lịch định kỳ.
- **Reference Data Cache:** Server-side ETag — khi admin cập nhật master data, backend broadcast SignalR `ReferenceDataChanged` để client invalidate cache.
- **Configuration Encryption:** Giá trị nhạy cảm (DB password, JWT key, SAP credentials) được mã hoá trong `appsettings.json`, giải mã lúc khởi động qua `EncryptDecrypt` class library.

### 3.2 Frontend — Tổ chức theo Feature/View

Frontend không áp dụng module federation hay micro-frontend. Cấu trúc là **feature-based views** trong một SPA duy nhất:

```
App.vue
  └── AppLayout.vue (Sidebar + Topbar)
        └── <router-view>
              ├── views/admin/*      — quản trị hệ thống
              ├── views/client/*     — màn hình khách hàng
              ├── views/auth/*       — đăng nhập / OTP
              └── views/common/*     — dùng chung
```

**Cách tổ chức state:**
- **Pinia** (`src/Pinia/`): state chính — filter per screen, user info, v.v.
- **Vuex** (`src/store/`): legacy — chỉ còn module auth, đang migration sang Pinia.

**API & Caching:**
- `src/api/api-main.js` — Axios instance duy nhất, tự động đính JWT header.
- `src/services/referenceDataCache.ts` — ETag + IndexedDB, transparent caching qua interceptor (không cần thay đổi component).

**Lazy loading toàn bộ:**
```js
component: () => import('@/views/admin/User/UserList.vue')
```

**Auto-import component:**  `unplugin-vue-components` + `PrimeVueResolver` — không cần `import` thủ công PrimeVue components trong template.

### 3.3 Database

- **ORM chính:** EF Core 9.0.4 với SQL Server provider.
- **DbContext:** `AppDbContext` kế thừa `IdentityDbContext<AppUser, AppRole, int, ...>`.
- **Quy mô:** 200+ `DbSet<T>` — schema rất lớn, bao gồm: users/roles, orders/documents, products, inventory, pricing, payments, approvals, notifications, SAP sync state.
- **Migrations:** Tự động apply khi startup (`Database.Migrate()`). Migration gần nhất: `20260428114002_Add_RefreshToken`.
- **Legacy ORM:** NHibernate 5.5.3 vẫn còn dependency trong solution.

### 3.4 Tích hợp ngoài

| Hệ thống | Mục đích | Thư viện/Cơ chế |
|---|---|---|
| SAP Business One | Đồng bộ ERP (đơn hàng, tồn kho, đối tác) | B1SLayer 2.1.0, Quartz sync jobs |
| VNPay | Cổng thanh toán | Custom service |
| OnePay | Cổng thanh toán (trả góp) | Custom service |
| Zalo OA | Nhắn tin + thanh toán | ZaloService, OAuth token |
| Office 365 SMTP | Email giao dịch | MailKit + MimeKit |
| SignalR | Thông báo real-time | NotificationHub, WebSocketHandler |

### 3.5 DevOps

- **Containerisation:** Docker multi-stage (build: sdk:8.0-jammy → runtime: aspnet:8.0-jammy), chạy với non-root user `apsp:1000`.
- **Docker Compose:** Chỉ dùng cho local dev (1 service `api`, port 8080).
- **CI/CD (GitHub Actions):**
  - Backend: build + test (.NET 8) + secrets scan (Gitleaks, alert-only).
  - Frontend: lint (ESLint 0-warning) + test (Vitest) + build (dev + uat matrix).
- **Observability:** OpenTelemetry 1.10.x (OTLP exporter + Prometheus) + Serilog daily rolling logs (30 ngày).

---

## 4. Điểm bất thường & đáng chú ý

### 4.1 Framework version mismatch

EF Core **9.0.4** được dùng với ASP.NET Core **8.0**. Đây là combination hợp lệ nhưng không thông thường — EF Core 9 thường đi kèm .NET 9. Cần chú ý compatibility khi nâng cấp framework.

### 4.2 Hai ORM song song

Cả **EF Core** (9.0.4) và **NHibernate** (5.5.3) đều có trong solution. Không rõ NHibernate còn được dùng thực sự hay chỉ còn là dependency thừa từ lịch sử. Cần xác minh thêm.

### 4.3 Hai state management song song (Frontend)

**Pinia** và **Vuex** cùng tồn tại. Vuex đang được loại bỏ (chỉ còn module auth) nhưng chưa hoàn tất. Code mới đang viết Pinia, code cũ vẫn dùng Vuex.

### 4.4 Hai Approval Workflow system song song

Cùng tồn tại `Service/Approval/` (V1) và `Service/Approval_V2/` — gợi ý đang trong quá trình refactor/migration. Chưa rõ V1 còn được gọi từ đâu.

### 4.5 AppDbContext quá lớn

200+ `DbSet<T>` trong một DbContext duy nhất. Không có bounded context hay schema separation. Khi codebase tiếp tục phát triển, đây có thể trở thành bottleneck về maintainability và migration conflict.

### 4.6 Test coverage gần như zero

`BackEndAPI.Tests` là empty project. Frontend test coverage chỉ target `src/helpers/`, `src/utils/`, `src/Pinia/`, `src/composables/` — toàn bộ views và services không có test. CI chạy test nhưng về bản chất không test gì cả.

### 4.7 Secrets scan chưa blocking

Gitleaks trong CI được set `continue-on-error: true` — nếu phát hiện secret trong code thì CI vẫn pass. Đây là chế độ alert-only, chưa có enforcement.

### 4.8 MailKit CVE đã biết

`MailKit`/`MimeKit` 4.14.0 có CVE moderate (`NU1902` warning bị suppress trong `.csproj`). Comment trong code ghi "tracking upstream" — đang chờ upstream fix.

### 4.9 JWT stored in localStorage (Frontend)

`src/helpers/auth-header.helper.js` đọc JWT từ `localStorage`. Đây là pattern phổ biến nhưng có rủi ro XSS nếu có third-party script inject vào trang.

### 4.10 i18n hoàn thành ~5% (Frontend)

Vue I18n đã setup, đã dịch common namespace + ~21 components. Còn **135+ files** trong views/admin và views/client chưa được i18n hoá (hardcode tiếng Việt). Đây là công việc dở dang quy mô lớn.

### 4.11 Sensitive values in appsettings.json (source-controlled)

`appsettings.json` chứa các giá trị mã hoá cho DB, SAP, payment gateways, email. Dù đã mã hoá, việc commit file này vào git vẫn là điểm rủi ro — nếu key giải mã bị lộ thì toàn bộ credentials bị compromised.

### 4.12 Vercel config cho Frontend

`frontend/vercel.json` tồn tại — gợi ý có lúc frontend được deploy lên Vercel, nhưng CI pipeline deploy lên đâu không rõ (không có deploy job trong workflow hiện tại).

---

## 5. Tóm tắt nhanh

| Tiêu chí | Đánh giá |
|---|---|
| Kiến trúc Backend | Layered Architecture — đơn giản, dễ hiểu nhưng ít abstraction boundary |
| Kiến trúc Frontend | Feature-based SPA — cấu trúc hợp lý cho quy mô hiện tại |
| Độ trưởng thành CI/CD | Trung bình — lint + build + test có, deploy chưa thấy |
| Test coverage | Rất thấp — gần như không có test thực chất |
| Observability | Tốt — OpenTelemetry + Serilog + Health checks |
| Security posture | Trung bình — JWT + Identity đầy đủ, một số điểm cần cải thiện |
| Technical debt | Đáng kể — 2 ORM, 2 approval system, 2 state manager, i18n dở dang |
| Quy mô domain | Lớn — B2B procurement, ERP sync, đa cổng thanh toán, real-time notifications |
