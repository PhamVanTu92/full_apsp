using BackEndAPI.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackEndAPI.Middleware;

/// <summary>
/// Result filter tự động bọc raw controller result thành ApiResponse&lt;T&gt;.
///
/// Hành vi:
///   • Action trả <c>Ok(dto)</c> hoặc <c>StatusCode(200, dto)</c> với dto KHÔNG phải
///     ApiResponse → bọc thành <c>ApiResponse&lt;T&gt;.Ok(dto)</c>.
///   • Action đã trả ApiResponse (qua ext methods OkResponse/etc) → để nguyên.
///   • Action trả <c>BadRequest("msg")</c> → bọc thành ApiResponse fail.
///   • Action trả <c>NotFound()</c> → bọc thành ApiResponse fail 404.
///   • Endpoint health/metrics → skip (không bọc — Prometheus parser không hiểu).
///
/// Mục tiêu: code legacy không cần sửa, vẫn nhận response chuẩn.
/// </summary>
public class ApiResponseFilter : IAsyncResultFilter
{
    private static readonly string[] _skippedPaths = { "/api/health", "/metrics" };

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var path = context.HttpContext.Request.Path.Value ?? "";
        if (_skippedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next();
            return;
        }

        // Đã là ObjectResult — kiểm tra payload có phải ApiResponse chưa
        if (context.Result is ObjectResult objResult)
        {
            var traceId = context.HttpContext.TraceIdentifier;

            if (objResult.Value is null)
            {
                // null payload → wrap thành Ok với data null (vẫn là success theo HTTP code)
                context.Result = new ObjectResult(WrapNullSuccess(traceId, objResult.StatusCode ?? 200))
                {
                    StatusCode = objResult.StatusCode ?? 200
                };
            }
            else if (!IsApiResponse(objResult.Value.GetType()))
            {
                var statusCode = objResult.StatusCode ?? 200;
                if (statusCode >= 200 && statusCode < 300)
                {
                    context.Result = new ObjectResult(Wrap(objResult.Value, statusCode, traceId))
                    {
                        StatusCode = statusCode
                    };
                }
                else
                {
                    // Error status (4xx/5xx) với payload custom — convert thành ApiResponse fail
                    context.Result = new ObjectResult(WrapError(objResult.Value, statusCode, traceId))
                    {
                        StatusCode = statusCode
                    };
                }
            }
            else
            {
                // Đã là ApiResponse — chỉ set traceId nếu chưa có
                SetTraceIdIfMissing(objResult.Value, traceId);
            }
        }
        else if (context.Result is StatusCodeResult statusOnly)
        {
            // Vd: NotFound(), NoContent() — không có body. Bọc thành ApiResponse.
            var sc = statusOnly.StatusCode;
            var traceId = context.HttpContext.TraceIdentifier;
            var resp = sc switch
            {
                204 => (object)ApiResponse.NoContent(),
                404 => ApiResponse.Fail(404, "NOT_FOUND", "Không tìm thấy"),
                401 => ApiResponse.Fail(401, "UNAUTHORIZED", "Chưa đăng nhập"),
                403 => ApiResponse.Fail(403, "FORBIDDEN", "Không có quyền"),
                _ when sc >= 400 => ApiResponse.Fail(sc, "ERROR", $"HTTP {sc}"),
                _ => Wrap<object?>(null, sc, traceId)
            };
            SetTraceIdIfMissing(resp, traceId);
            context.Result = new ObjectResult(resp) { StatusCode = sc };
        }
        // Các loại khác (FileResult, RedirectResult, ...) để nguyên.

        await next();
    }

    private static bool IsApiResponse(Type t)
    {
        if (t == typeof(ApiResponse)) return true;
        var cur = t;
        while (cur != null && cur != typeof(object))
        {
            if (cur.IsGenericType && cur.GetGenericTypeDefinition() == typeof(ApiResponse<>))
                return true;
            cur = cur.BaseType;
        }
        return false;
    }

    private static object Wrap<T>(T data, int statusCode, string traceId)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Code = statusCode == 201 ? "CREATED" : "OK",
            Message = "Success",
            Data = data,
            TraceId = traceId
        };
    }

    private static object WrapNullSuccess(string traceId, int statusCode)
        => new ApiResponse<object>
        {
            Success = true,
            StatusCode = statusCode,
            Code = "OK",
            Message = "Success",
            Data = null,
            TraceId = traceId
        };

    private static object WrapError(object payload, int statusCode, string traceId)
    {
        // Nếu payload là string → dùng làm message
        // Nếu payload là object có property "message" → dùng property đó
        // Còn lại → serialize làm message
        var message = payload switch
        {
            string s => s,
            ProblemDetails pd => pd.Detail ?? pd.Title ?? "Error",
            _ => System.Text.Json.JsonSerializer.Serialize(payload)
        };

        return new ApiResponse<object>
        {
            Success = false,
            StatusCode = statusCode,
            Code = statusCode switch
            {
                400 => "VALIDATION_ERROR",
                401 => "UNAUTHORIZED",
                403 => "FORBIDDEN",
                404 => "NOT_FOUND",
                _ => "ERROR"
            },
            Message = message,
            TraceId = traceId
        };
    }

    private static void SetTraceIdIfMissing(object resp, string traceId)
    {
        var prop = resp.GetType().GetProperty("TraceId");
        if (prop != null && (prop.GetValue(resp) as string) == null)
            prop.SetValue(resp, traceId);
    }
}
