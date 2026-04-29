# API Response Format — chuẩn chung toàn project

Mọi HTTP endpoint của BackEndAPI **bắt buộc** trả response theo cấu trúc envelope sau.

## Format

```json
{
  "success": true,
  "statusCode": 200,
  "code": "OK",
  "message": "Success",
  "data": { /* payload thật */ },
  "errors": ["err1", "err2"],
  "traceId": "00-abc...",
  "timestamp": "2026-04-28T03:00:00.000Z"
}
```

| Field | Kiểu | Mô tả |
|---|---|---|
| `success` | bool | true cho 2xx, false cho 4xx/5xx |
| `statusCode` | int | HTTP status code (lặp lại cho client dễ parse) |
| `code` | string | Mã semantic: `OK`, `CREATED`, `NOT_FOUND`, `VALIDATION_ERROR`, `BUSINESS_ERROR`, `UNAUTHORIZED`, `FORBIDDEN`, `INTERNAL` |
| `message` | string | Human-readable, có thể tiếng Việt |
| `data` | T \| null | Payload — null cho error |
| `errors` | string[] \| null | Validation errors hoặc multi-error list — null nếu không có |
| `traceId` | string \| null | Để correlate với log Serilog |
| `timestamp` | DateTime | UTC, set tự động |

`errors` và `traceId` được **omit khỏi JSON nếu null** (JsonIgnoreCondition.WhenWritingNull).

## Cách dùng trong controller

### Cách 1 — Extension methods (recommended)

```csharp
using BackEndAPI.Extensions;

[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    var dto = await _service.GetAsync(id);
    if (dto == null)
        return this.NotFoundResponse($"Doc {id} không tồn tại");

    return this.OkResponse(dto);
}

[HttpPost]
public async Task<IActionResult> Create(CreateDto dto)
{
    var result = await _service.CreateAsync(dto);
    return this.CreatedResponse(result, "Tạo thành công");
}
```

Available extensions trên `ControllerBase`:
- `OkResponse<T>(data, message?)` → 200
- `CreatedResponse<T>(data, message?)` → 201
- `NoContentResponse(message?)` → 200 với data null
- `NotFoundResponse(message)` → 404
- `BadRequestResponse(message, errors?)` → 400
- `BusinessErrorResponse(code, message, statusCode = 400)` → custom
- `UnauthorizedResponse(message?)` → 401
- `ForbiddenResponse(message?)` → 403

### Cách 2 — Code legacy `Ok(dto)` vẫn hoạt động (tự động bọc)

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    var dto = await _service.GetAsync(id);
    return Ok(dto);  // ApiResponseFilter auto-wrap → ApiResponse<T>.Ok(dto)
}
```

`ApiResponseFilter` (đăng ký global) tự bọc:
- `Ok(dto)` → `ApiResponse<T>.Ok(dto)`
- `BadRequest("msg")` → `ApiResponse.Fail(400, "VALIDATION_ERROR", "msg")`
- `NotFound()` → `ApiResponse.Fail(404, "NOT_FOUND", "Không tìm thấy")`
- `StatusCode(500, payload)` → `ApiResponse.Fail(500, "ERROR", ...)`

→ **Code cũ KHÔNG cần sửa** để tương thích format mới. Chỉ khuyến khích migrate dần sang Cách 1 vì rõ ràng + custom message dễ hơn.

### Cách 3 — Throw BusinessException cho error nghiệp vụ

```csharp
if (totalAvailablePoints < pointsNeeded)
    throw new BusinessException("INSUFFICIENT_POINTS", "Không đủ điểm để đổi quà",
        HttpStatusCode.BadRequest);
```

`ExceptionHandlingMiddleware` bắt và convert thành ApiResponse với `code` từ `ErrorCode`.

## Ví dụ output

### Success — GET /api/document/123

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "statusCode": 200,
  "code": "OK",
  "message": "Success",
  "data": {
    "id": 123,
    "invoiceCode": "DH00003823",
    "total": 17350200
  },
  "traceId": "00-abc123-def456-00",
  "timestamp": "2026-04-28T03:00:00.000Z"
}
```

### Validation error — POST /api/order với body sai

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "success": false,
  "statusCode": 400,
  "code": "VALIDATION_ERROR",
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "CardCode is required",
    "Quantity must be positive"
  ],
  "traceId": "00-...",
  "timestamp": "..."
}
```

### Business error — POST /api/redeem với điểm không đủ

```http
HTTP/1.1 400 Bad Request

{
  "success": false,
  "statusCode": 400,
  "code": "INSUFFICIENT_POINTS",
  "message": "Không đủ điểm để đổi vật phẩm / Chương trình đã hết hạn",
  "traceId": "00-...",
  "timestamp": "..."
}
```

### Unhandled exception (Production)

```http
HTTP/1.1 500 Internal Server Error

{
  "success": false,
  "statusCode": 500,
  "code": "INTERNAL",
  "message": "Internal server error. Vui lòng liên hệ hỗ trợ với mã traceId.",
  "traceId": "00-abc123-...",
  "timestamp": "..."
}
```

→ Admin tìm trong Serilog log: `grep "00-abc123" Logs/log-*.txt` để biết chính xác bug.

## Endpoint không bị wrap

- `/api/health/*` — health check format chuẩn riêng (UIResponseWriter)
- `/metrics` — Prometheus text format

Các endpoint khác đều phải theo format này.

## Migration cho client (frontend)

Trước:
```ts
const res = await api.get('/api/document/1');
console.log(res.data);  // dto thật
```

Sau:
```ts
const res = await api.get('/api/document/1');
console.log(res.data.data);  // dto thật nằm ở .data.data
console.log(res.data.success); // true/false
console.log(res.data.message); // hiển thị notification
```

Đề xuất tạo axios interceptor:
```ts
axios.interceptors.response.use(
  (res) => {
    if (res.data?.success === false) {
      throw new ApiError(res.data);
    }
    res.data = res.data?.data ?? res.data;  // unwrap envelope
    return res;
  },
  (err) => {
    if (err.response?.data?.success === false) {
      // error envelope
      throw new ApiError(err.response.data);
    }
    throw err;
  }
);

class ApiError extends Error {
  constructor(public response: { code: string; message: string; errors?: string[]; traceId?: string }) {
    super(response.message);
  }
}
```

## Migration cho controllers cũ

Không bắt buộc migrate ngay — `ApiResponseFilter` đã handle. Nhưng khuyến khích:

1. **Tuần 1-2**: tất cả PR mới phải dùng `OkResponse / NotFoundResponse / BusinessErrorResponse`
2. **Tuần 3-4**: refactor controllers thường xuyên touch
3. **Tuần 5+**: dọn dần controllers cũ

Pattern thay thế:

| Trước | Sau |
|---|---|
| `return Ok(dto)` | `return this.OkResponse(dto)` |
| `return BadRequest(new { error = "..." })` | `return this.BadRequestResponse("...")` |
| `return NotFound()` | `return this.NotFoundResponse("Item không tồn tại")` |
| `return Ok(new { mess.Status, mess.Errors })` | `throw new BusinessException(...)` |

## Checklist code review

- [ ] Controller method trả `IActionResult`, không trả raw DTO
- [ ] Không trả `Ok(null)` — dùng `NotFoundResponse` hoặc `NoContentResponse`
- [ ] Error nghiệp vụ throw `BusinessException` thay vì return Mess
- [ ] Validation errors gom vào `errors[]`, không nhồi vào message
- [ ] Custom error code phải UPPER_SNAKE_CASE và document trong wiki
