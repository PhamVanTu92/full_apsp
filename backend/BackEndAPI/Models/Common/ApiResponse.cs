using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Common;

/// <summary>
/// Envelope chuẩn cho mọi HTTP response (success + error) trên toàn project.
///
/// Format thống nhất:
/// <code>
/// {
///   "success": true,
///   "statusCode": 200,
///   "code": "OK",
///   "message": "...",
///   "data": { ... },
///   "errors": null,
///   "traceId": "00-...",
///   "timestamp": "2026-04-28T03:00:00Z"
/// }
/// </code>
///
/// Cách dùng trong controller:
/// <code>
/// return this.OkResponse(dto);                        // 200 OK + data
/// return this.OkResponse(dto, "Tạo thành công");      // 200 OK + data + custom message
/// return this.NotFoundResponse("Doc 123 không tồn tại");
/// return this.BadRequestResponse("Validation fail", validationErrors);
/// </code>
///
/// Hoặc auto-wrap: trả về <c>Ok(dto)</c> trực tiếp — ResultFilter tự bọc thành ApiResponse.
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    /// <summary>Mã code semantic — "OK", "NOT_FOUND", "VALIDATION_ERROR", "BUSINESS_ERROR", v.v.</summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = "OK";

    /// <summary>Message human-readable, có thể tiếng Việt.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Payload thật. Null cho error response.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>Validation errors hoặc multi-error list. Null nếu không có.</summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    /// <summary>Trace ID để correlate với log. Set tự động bởi middleware.</summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ── Factory methods ────────────────────────────────────────────────

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        StatusCode = 200,
        Code = "OK",
        Message = message ?? "Success",
        Data = data
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        StatusCode = 201,
        Code = "CREATED",
        Message = message ?? "Created",
        Data = data
    };

    public static ApiResponse<T> Fail(int statusCode, string code, string message, List<string>? errors = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Code = code,
        Message = message,
        Data = default,
        Errors = errors
    };
}

/// <summary>Non-generic helpers cho response không có data (vd: 204 NoContent).</summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse NoContent(string? message = null) => new()
    {
        Success = true,
        StatusCode = 204,
        Code = "NO_CONTENT",
        Message = message ?? "No content"
    };

    public static new ApiResponse Fail(int statusCode, string code, string message, List<string>? errors = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Code = code,
        Message = message,
        Errors = errors
    };
}
