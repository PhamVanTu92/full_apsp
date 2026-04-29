# INTEGRATION REVIEW — Backend ↔ Frontend

> **Ngày review:** 2026-04-29  
> **Phạm vi:** Cách `backend/` (.NET 8) và `frontend/` (Vue 3 + Vite) tương tác sau khi gộp monorepo.  
> **Phương pháp:** Đối chiếu trực tiếp file backend ↔ file frontend, không sửa code.  
> **Tổ chức:** 6 nhóm theo yêu cầu, mỗi finding có mức độ Critical / High / Medium / Low.

---

## 📊 Tổng hợp

| Nhóm | Critical | High | Medium | Low |
|---|---|---|---|---|
| 1. API contract | 0 | 2 | 2 | 0 |
| 2. CORS | 2 | 0 | 0 | 0 |
| 3. Auth flow | 0 | 1 | 2 | 0 |
| 4. Error format | 0 | 2 | 1 | 0 |
| 5. Duplicate logic | 0 | 0 | 3 | 1 |
| 6. Environment config | 1 | 0 | 2 | 1 |
| **Tổng** | **3** | **5** | **10** | **2** |

---

## 1. API contract — Response shape có khớp không?

### ✅ Phần đã đúng

Backend wrap mọi response qua `ApiResponseFilter` ([Middleware/ApiResponseFilter.cs:20-165](backend/BackEndAPI/Middleware/ApiResponseFilter.cs)) thành envelope chuẩn:

```json
{
  "success":   bool,
  "statusCode": int,
  "code":      "OK|NOT_FOUND|VALIDATION_ERROR|INTERNAL|...",
  "message":   string,
  "data":      T,
  "errors":    string[] | null,
  "traceId":   string | null,
  "timestamp": "ISO-8601"
}
```

Frontend tự động unwrap envelope ở [src/api/api-main.ts:107-111](frontend/src/api/api-main.ts):

```typescript
const env = response.data as any;
if (env && typeof env === 'object' && 'success' in env) {
    response.data = env.data ?? null;
}
```

→ Component nhận trực tiếp `response.data = env.data`. **Khớp tốt.**

---

### I-1.1 [HIGH] — Pagination field naming không thống nhất

**Backend:** [backend/BackEndAPI/Dtos/Pagination.cs:3-8](backend/BackEndAPI/Dtos/Pagination.cs)

```csharp
public class Pagination {
    public object? Result { get; set; }    // ❗ tên là Result
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}
```

**Frontend:** Một số component đọc `response.data.items` (legacy), số khác đọc `response.data.Result`. Ví dụ [src/views/admin/MasterData/AgenMan/AgencyCategory.vue:15-16](frontend/src/views/admin/MasterData/AgenMan/AgencyCategory.vue):

```typescript
suppliersData.items   // mong đợi field 'items'
pagable.total         // chữ thường, nhưng backend trả 'Total'
```

**Vì sao:** JsonSerializer mặc định ASP.NET Core dùng camelCase → `Result` → `result`, `Total` → `total`. Frontend mỗi nơi truy cập một kiểu (`items`, `result`, `Result`) tuỳ DTO. Bug khó debug, có chỗ luôn ra null.

**Đề xuất:**
- Đổi `Pagination` thành generic và đổi tên field về `items`:
```csharp
public class PagedResult<T> { public List<T> Items {get;set;} = new(); public int Page {get;set;} public int PageSize {get;set;} public int Total {get;set;} }
```
- Audit toàn frontend grep `response.data.items|.Result|.result` → chuẩn hoá còn 1 field.

---

### I-1.2 [HIGH] — Không có DTO → TypeScript codegen

**Backend:** ~200+ DTO class trong [backend/BackEndAPI/Dtos/](backend/BackEndAPI/Dtos/).  
**Frontend:** TypeScript interface viết tay rời rạc. 44 vị trí `: any` (đã ghi trong FRONTEND_REVIEW.md). `[key: string]: any` ở `AppUser`/`AuthUser` ([src/Pinia/auth.ts:16, 24](frontend/src/Pinia/auth.ts)).

**Vì sao:** Khi backend đổi DTO (rename field, thêm bớt property), frontend không có compile-time error → bug chỉ phát hiện ở runtime. 50% file frontend vẫn là JS, không có type chặn lại.

**Đề xuất:** Bật Swashbuckle XML doc đã có sẵn → dùng **NSwag** hoặc **openapi-typescript** generate `src/types/api.ts` từ `/swagger/v1/swagger.json`. Add CI step:
```bash
npx openapi-typescript http://localhost:5279/swagger/v1/swagger.json -o src/types/api.ts
```

---

### I-1.3 [MEDIUM] — Một số endpoint custom không đi qua envelope

**Backend:** [Controllers/AccountController.cs:112](backend/BackEndAPI/Controllers/AccountController.cs)

```csharp
return BadRequest(new { mess.Status, mess.Errors });   // ❌ tự định nghĩa shape
```

ApiResponseFilter sẽ wrap lại lớp ngoài, nhưng `data` lại là object `{Status, Errors}` không tuân theo convention. Frontend không biết phải đọc `data.Status` hay `errors` cấp ngoài.

**Đề xuất:** Throw `BusinessException` thay vì tự build BadRequest object → ExceptionHandlingMiddleware tự wrap chuẩn.

---

### I-1.4 [MEDIUM] — Health check & metrics endpoint exempt khỏi envelope

**Backend:** [Middleware/ApiResponseFilter.cs](backend/BackEndAPI/Middleware/ApiResponseFilter.cs) skip `/api/health` và `/metrics`.

**Vì sao OK:** Đúng convention (Prometheus, K8s liveness probe yêu cầu format riêng). Cần document rõ trong README để frontend dev không hiểu nhầm.

---

## 2. CORS — Backend có whitelist đúng origin frontend không?

### I-2.1 [CRITICAL] — `Cors:AllowedOrigins` vẫn để placeholder

**Backend:** [appsettings.json:12-17](backend/BackEndAPI/appsettings.json)

```json
"Cors": {
  "_comment": "Production: liệt kê origin được phép gọi API",
  "AllowedOrigins": [
    "https://example.com",          // ❌ placeholder
    "https://admin.example.com"
  ]
}
```

**Đối chiếu với frontend:**
- `.env.uat`: `http://160.30.252.14:8070/api/` → origin gọi từ `http://160.30.252.14:8070`
- `.env.production`: `https://portal.apsaigonpetro.com:8023/api/` → origin `https://portal.apsaigonpetro.com:8023`

→ Origin frontend **không nằm trong whitelist** backend. Khi deploy thật:
- Hoặc CORS reject toàn bộ request frontend → hệ thống chết.
- Hoặc đã được override bằng `appsettings.Production.json` riêng (chưa thấy trong repo) → không version-controlled, dễ mất khi setup môi trường mới.

**Vì sao Critical:** Đây là rào cản block deploy production. Chỉ cần một đợt deploy quên override config là toàn site call API đều bị browser block.

**Đề xuất:** Tạo `appsettings.UAT.json` và `appsettings.Production.json` trong repo (commit), hoặc inject qua environment variable:
```bash
Cors__AllowedOrigins__0=https://portal.apsaigonpetro.com:8023
```

---

### I-2.2 [CRITICAL] — Frontend gửi JWT trực tiếp ra `fox.ai.vn` (cross-site)

**Frontend:** [src/views/client/pages/PurchaseOrder/views/CheckOut.vue:479-485, 572-575](frontend/src/views/client/pages/PurchaseOrder/views/CheckOut.vue)

```typescript
const ress = await axios.post(
    "https://fox.ai.vn/vnpay_php/vnpay_create_payment.php",
    { ...dataform, token: authHeader_new().Authorization }
);
```

**Vì sao Critical:** JWT của APSP bị gửi sang domain ngoài (không thuộc `Cors:AllowedOrigins` của APSP, không nằm trong scope JWT Issuer/Audience). Hậu quả:
- Token bị log ở server `fox.ai.vn` (PHP).
- Nếu `fox.ai.vn` bị compromised → toàn bộ token trong khoảng đó bị lộ.
- Cookie `PHPSESSID` set lại từ `fox.ai.vn` về document.cookie → session fixation (đã ghi ở FRONTEND_REVIEW C-3).

→ Đây là vấn đề **cross-cutting**: backend không thể tin tưởng client request từ `fox.ai.vn` (không trong CORS list), nhưng frontend lại tự đẩy luồng thanh toán ra ngoài.

**Đề xuất:** Toàn bộ luồng VNPay phải đi qua backend APSP. Backend là client của VNPay/fox.ai.vn, frontend chỉ giao tiếp với backend APSP.

---

## 3. Authentication flow — JWT, refresh token, SignalR

### ✅ Phần đã đúng

- **Refresh token rotation:** [Controllers/AccountController.cs:142-172](backend/BackEndAPI/Controllers/AccountController.cs) rotate refresh token cũ → cấp mới + access mới. Frontend [src/api/api-main.ts:34-97](frontend/src/api/api-main.ts) có queue chống 401 storm.
- **Logout:** Backend revoke refresh token ở [AccountController.cs:178-186](backend/BackEndAPI/Controllers/AccountController.cs); frontend xoá localStorage + gọi endpoint.
- **SignalR auth:** [Program.cs:625] (backend) đọc query string `access_token`, frontend [src/services/referenceDataHub.ts:46-47](frontend/src/services/referenceDataHub.ts) truyền qua `accessTokenFactory: () => getToken() ?? ''`. Khớp đúng.

---

### I-3.1 [HIGH] — JWT Issuer/Audience vẫn là `localhost`

**Backend:** [appsettings.json:28-29](backend/BackEndAPI/appsettings.json)

```json
"Jwt": {
  "Issuer":   "https://localhost",
  "Audience": "http://localhost"
}
```

**Backend Program.cs có guard:** [Program.cs:130-138](backend/BackEndAPI/Program.cs) — nếu non-Development mà chứa `localhost` thì throw startup. Tốt — nhưng nghĩa là khi deploy UAT/Prod sẽ **crash khi start** nếu chưa override config. Tương tự CORS, chưa thấy `appsettings.Production.json` trong repo.

**Đề xuất:** Tạo `appsettings.{UAT,Production}.json` set Issuer/Audience đúng domain, commit vào repo (không chứa secret, chỉ host).

---

### I-3.2 [MEDIUM] — Token lifetime không tường minh, frontend không sync

**Backend:** Không tìm thấy `Jwt:ExpiresInMinutes` hoặc tương đương trong appsettings — token lifetime hardcode trong `AccountService.GenerateJSONWebToken` (phải đọc service mới biết). Comment ở [AccountController.cs:176](backend/BackEndAPI/Controllers/AccountController.cs) ghi "TTL 30 phút".

**Frontend:** [src/Pinia/auth.ts:55](frontend/src/Pinia/auth.ts) có field `access-exp` nhưng không thấy logic refresh proactive trước khi expire — chỉ refresh **reactive** sau khi nhận 401.

**Vì sao:** 
- Nếu access token TTL ngắn (5-10 phút), mọi request đầu tiên sau khi tab idle đều phải retry sau 401 → user thấy delay.
- Nếu TTL dài (8h+) thì rủi ro bảo mật khi token bị lộ.

**Đề xuất:**
- Đưa lifetime vào config: `"Jwt:AccessTokenMinutes": 15, "Jwt:RefreshTokenDays": 7`.
- Frontend lưu `expiresAt` rồi proactive refresh khi còn ~30 giây hết hạn (interceptor trước request thay vì retry sau 401).

---

### I-3.3 [MEDIUM] — Refresh token cũng nằm trong localStorage cùng JWT

Đã chi tiết ở FRONTEND_REVIEW C-2. Trong góc nhìn integration: backend hỗ trợ rotation đầy đủ, **rủi ro nằm ở phía frontend** (client storage). Backend có thể giúp bằng cách trả refresh token qua `Set-Cookie: HttpOnly; Secure; SameSite=Strict` thay vì JSON body. Cần phối hợp 2 bên đổi cùng lúc.

---

## 4. Error response format

### I-4.1 [HIGH] — `BadRequest(ModelState)` bypass envelope ở một số controller

**Backend:** [Controllers/AccountController.cs:104-106](backend/BackEndAPI/Controllers/AccountController.cs)

```csharp
if (!ModelState.IsValid) {
    return BadRequest(ModelState);  // ❌ ModelState raw
}
```

`ApiResponseFilter` thấy `BadRequestObjectResult` thì wrap lại, nhưng nội dung `data` là dictionary lồng nhau (key = field name, value = string array các lỗi). Trong khi các controller khác trả `errors: ["msg1", "msg2"]` (string array phẳng).

**Frontend:** [src/Pinia/auth.ts:70](frontend/src/Pinia/auth.ts)

```typescript
const errorMessage = error.response?.data?.errors || 'An error occurred during login.';
```

Frontend **giả định** `errors` là `string[]`. Nếu nhận về dictionary của ModelState thì cách hiển thị sai (in ra `[object Object]`).

**Đề xuất:** Thêm action filter `[ValidateModelState]` global hoặc dùng `ProblemDetails` chuẩn RFC 7807, sau đó ExceptionHandlingMiddleware chuyển thành `ApiResponse.Fail` với `errors: string[]` flatten từ ModelState.

```csharp
var errors = ModelState
    .Where(kv => kv.Value!.Errors.Count > 0)
    .SelectMany(kv => kv.Value!.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
    .ToList();
return BadRequest(ApiResponse.Fail(400, "VALIDATION_ERROR", "Validation failed", errors));
```

---

### I-4.2 [HIGH] — Frontend không phân biệt `ApiError` vs raw axios error

**Frontend:** [src/api/api-main.ts:113-125](frontend/src/api/api-main.ts) đã có `ApiError` class wrap envelope. Tuy nhiên nhiều component vẫn dùng pattern cũ:

```typescript
catch (error) {
    toast.add({ detail: error.message });   // ❌ message của Axios, không phải message từ backend
}
```

Hoặc đọc `error.response.data.errors` trực tiếp ([src/Pinia/auth.ts:70](frontend/src/Pinia/auth.ts)) — bỏ qua `ApiError`.

**Đề xuất:** Tạo composable `useAsyncTask()` chuẩn hoá:
```typescript
catch (e) {
   if (e instanceof ApiError) toast.error(e.message, e.errors);
   else toast.error('Lỗi kết nối');
}
```
Audit toàn frontend, replace pattern cũ.

---

### I-4.3 [MEDIUM] — TraceId không được hiển thị/log ở frontend

**Backend:** Trả `traceId` trong mọi `ApiResponse` (xem ApiResponseFilter).  
**Frontend:** `ApiError` có lưu `traceId` ([api-main.ts:10-30](frontend/src/api/api-main.ts)) nhưng không hiển thị cho user và không log lên error tracking (Sentry chưa setup).

**Vì sao:** Khi user báo lỗi, support không có traceId để tra log → debug khó.

**Đề xuất:** Toast lỗi hiển thị traceId nhỏ dưới message ("Mã lỗi: abc-123-..."), kèm copy button. Hoặc gửi lên Sentry qua interceptor.

---

## 5. Duplicate logic giữa 2 phía

### I-5.1 [MEDIUM] — Validation rule không sync

**Backend:** DataAnnotation `[Required]`, `[StringLength]`, `[EmailAddress]` rải trong DTO.  
**Frontend:** Có form validation thông qua PrimeVue `:invalid` (mỗi component tự viết) nhưng không generate từ DTO. Một số form chỉ validate khi submit (FRONTEND_REVIEW M-9).

**Vì sao:** 
- Backend đổi rule (ví dụ password tối thiểu 8 ký tự), frontend không tự update → user thấy backend reject sau khi nhấn submit.
- Frontend đôi khi validate **chặt hơn** backend, đôi khi **lỏng hơn** → trải nghiệm không đoán trước được.

**Đề xuất:** Sau khi có OpenAPI codegen (I-1.2), generate kèm validation schema (Zod / Yup) từ JSON Schema. Hoặc tối thiểu document rule rõ ràng và sync thủ công.

---

### I-5.2 [MEDIUM] — DateTime timezone

**Backend:** [Program.cs:292](backend/BackEndAPI/Program.cs) dùng `ConvertDateTimeToUTC` converter → tất cả DateTime serialize ra UTC.

**Frontend:** Không có timezone config tường minh; date-fns dùng locale machine (browser của user).

**Vì sao:** OK với client cùng múi giờ Việt Nam, **nguy hiểm** với client ở múi giờ khác (ví dụ user ở nước ngoài thấy ngày lệch). Cũng nguy hiểm với date-only field (sinh nhật, deadline) — convert UTC có thể nhảy 1 ngày.

**Đề xuất:** 
- Document rõ: backend trả UTC ISO-8601, frontend parse rồi format theo `Asia/Ho_Chi_Minh`.
- Date-only field nên dùng `string "YYYY-MM-DD"` không phải DateTime.

---

### I-5.3 [MEDIUM] — Status code (string enum) hardcode 2 phía

**Backend:** Status string như `"DXN"`, `"D"`, `"Open"`, `"PEND"`, `"FC"`, `"YCLH"` xuất hiện trong DocumentService (xem BACKEND_REVIEW M-5).  
**Frontend:** Hardcode lại các string này trong template/script.

**Vì sao:** Đổi 1 mã ở backend mà quên frontend (hoặc ngược lại) → đơn không hiện đúng trạng thái. Không có single source of truth.

**Đề xuất:** Tạo enum ở backend, expose qua endpoint `/api/enums` hoặc generate cùng OpenAPI codegen. Frontend import từ generated file thay vì hardcode.

---

### I-5.4 [LOW] — Tính giá / khuyến mãi

**Backend:** `PromotionCalculatorService` ([Program.cs:222](backend/BackEndAPI/Program.cs)) là source of truth.  
**Frontend:** Có preview tính giá (cart, OutputCheck, OrderSummary) — không rõ có tính lại y hệt hay chỉ display.

**Vì sao Low:** Pattern "FE preview, BE authoritative" là chuẩn đẹp **nếu** logic FE chỉ là approximation và backend luôn là final. Cần verify không có chỗ frontend **đè** giá lên backend (ví dụ user sửa input giá → submit → backend tin tưởng giá user gửi).

**Đề xuất:** Audit endpoint Order/Cart submit ở backend xem có recalculate không. Nếu chưa, thêm test integration.

---

## 6. Environment configuration

### I-6.1 [CRITICAL] — Thiếu `appsettings.{UAT,Production}.json` trong repo

**Backend:** Chỉ có [appsettings.json](backend/BackEndAPI/appsettings.json) (default + dev) và [appsettings.example.json](backend/BackEndAPI/appsettings.example.json). Không có `appsettings.UAT.json`, `appsettings.Production.json`.

Trong khi đó:
- JWT Issuer/Audience cần override (I-3.1)
- CORS AllowedOrigins cần override (I-2.1)
- ConnectionString khác nhau

**Vì sao Critical:** Bất cứ ai deploy môi trường mới (UAT/Prod/staging) phải **biết** tay set tất cả env var hoặc tự tạo file riêng. Onboarding rất khó. Production startup sẽ throw nếu thiếu.

**Đề xuất:** Commit template `appsettings.UAT.json`, `appsettings.Production.json` vào repo (chỉ chứa host/url/cors, không chứa secret). Secret để env var.

---

### I-6.2 [MEDIUM] — UAT dùng HTTP + IP raw, Prod dùng HTTPS + custom port

**Frontend env:**

| File | API URL |
|---|---|
| `.env.development` | `/api/` (vite proxy) |
| `.env.uat` | `http://160.30.252.14:8070/api/` ⚠ HTTP + IP |
| `.env.production` | `https://portal.apsaigonpetro.com:8023/api/` ⚠ port 8023 |

**Vì sao:**
- HTTP ở UAT → token JWT bay plaintext qua mạng. Mọi tester có thể sniff.
- Port lẻ 8023 ở prod → một số firewall doanh nghiệp/trường học block → user không truy cập được.
- IP raw ở UAT khó revoke/rotate, không có DNS audit trail.

**Đề xuất:** UAT cũng phải HTTPS + DNS (`uat-apsp.apsaigonpetro.internal`). Prod dùng port 443 chuẩn (qua reverse proxy).

---

### I-6.3 [MEDIUM] — WebSocket host có thể tách rời HTTP host

**Frontend:**
```
.env.production:
VITE_APP_API     = https://portal.apsaigonpetro.com:8023/api/
VITE_APP_WS_HOST = wss://portal.apsaigonpetro.com:8023
```

**Vite proxy dev:** API → `http://localhost:5279`, WS → `ws://localhost:5279` (cùng host).

**Vì sao:** Hiện tại 2 URL **trùng host:port** nên không vấn đề. Nhưng cấu trúc 2 biến tách rời mở khả năng chia khác nhau (ví dụ WS qua subdomain riêng `wss://realtime.apsp...`). Khi đó cần check thêm CORS policy cho SignalR + cấu hình SignalR cluster (sticky session).

**Đề xuất:** Document rõ: nếu tách host phải bật Redis backplane cho SignalR và CORS policy SignalR riêng.

---

### I-6.4 [LOW] — Thiếu `.env.example` đối xứng cho backend

**Frontend:** Có `.env.example`.  
**Backend:** Có [appsettings.example.json](backend/BackEndAPI/appsettings.example.json) — tốt, nhưng không có `.env.example` cho `docker-compose.yml` (mặc dù compose đọc env từ `.env`).

**Đề xuất:** Tạo `backend/.env.example` liệt kê tất cả env override quan trọng:
```
ConnectionStrings__DefaultConnection=...
Jwt__Key=...
Jwt__Issuer=...
Cors__AllowedOrigins__0=...
SAPServiceLayer__Host=...
```

---

## 🎯 Kế hoạch hành động

| Thứ tự | Hạng mục | Effort | Phía cần phối hợp |
|---|---|---|---|
| **P0 — trước deploy UAT/Prod** | I-6.1 + I-2.1 + I-3.1 commit `appsettings.{UAT,Production}.json` đúng host & whitelist | 2h | Backend + DevOps |
| **P0** | I-2.2 chuyển luồng VNPay đi qua backend APSP, không gửi JWT ra `fox.ai.vn` | 8h | Backend + Frontend |
| **P1 — sprint này** | I-4.1 wrap `BadRequest(ModelState)` thành `ApiResponse.Fail` với errors phẳng | 2h | Backend |
| **P1** | I-4.2 audit frontend dùng `ApiError` thay vì `error.message` raw | 4h | Frontend |
| **P1** | I-3.2 đưa token lifetime vào config + proactive refresh ở frontend | 4h | Cả hai |
| **P1** | I-1.1 chuẩn hoá Pagination shape (`items` thay vì `Result`) | 4h | Cả hai |
| **P2 — sprint sau** | I-1.2 setup OpenAPI → TypeScript codegen | 8h | Cả hai |
| **P2** | I-5.3 enum status code shared qua codegen | 3h | Cả hai |
| **P2** | I-6.2 UAT chuyển HTTPS + DNS, prod đổi port 443 | DevOps |
| **P2** | I-3.3 chuyển refresh token sang HttpOnly cookie | 8h | Cả hai |
| **P3 — backlog** | I-4.3 traceId hiển thị + Sentry, I-5.1 validation schema sync, I-5.2 timezone doc, I-5.4 audit price calc, I-6.3 WS doc, I-6.4 backend `.env.example` | rải rác | Cả hai |

---

## 🔍 Kết luận tổng

**Phần khung tốt:**
- ApiResponseFilter + axios interceptor unwrap envelope: **khớp chuẩn**.
- Refresh token rotation, SignalR auth: **đúng pattern**.
- Backend có guard hard-fail nếu config sai (Issuer chứa `localhost` ở prod, CORS rỗng ở prod) — bắt lỗi sớm khi deploy.

**Phần chính cần xử lý:**
- **Config gap:** chưa có `appsettings.{UAT,Production}.json` trong repo → cả CORS lẫn JWT đều sẽ chặn deploy. Đây là vấn đề P0 cụ thể nhất.
- **Cross-domain payment flow:** VNPay đi qua `fox.ai.vn` là điểm yếu lớn nhất về trust boundary (cấu kết với C-2 và C-3 của FRONTEND_REVIEW thành chuỗi tấn công khả thi).
- **Type safety gap:** thiếu DTO codegen → 50% frontend là JS, runtime mới phát hiện schema mismatch. Đầu tư 1 lần cài OpenAPI generator giải quyết được nhiều dạng bug cùng lúc.
- **Error format inconsistency:** ModelState raw vs ApiResponse → frontend phải defensive với mọi shape có thể có.

Sau khi xử lý xong P0 + P1, hệ thống tích hợp 2 phía sẽ ở trạng thái production-ready. P2 (codegen, HttpOnly cookie, DNS) là đầu tư trung hạn để giảm bug rate và rủi ro bảo mật bền vững.
