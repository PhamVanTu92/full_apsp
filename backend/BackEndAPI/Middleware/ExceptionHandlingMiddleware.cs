using System.Net;
using System.Text.Json;
using BackEndAPI.Models.Common;
using Microsoft.AspNetCore.Hosting;

namespace BackEndAPI.Middleware;

/// <summary>
/// Bắt mọi exception không handle, log đầy đủ qua Serilog, trả ApiResponse JSON.
///
/// Format chuẩn (giống ApiResponseFilter):
/// <code>
/// {
///   "success": false,
///   "statusCode": 500,
///   "code": "INTERNAL",
///   "message": "Internal server error. TraceId=...",
///   "traceId": "...",
///   "timestamp": "..."
/// }
/// </code>
///
/// Production: KHÔNG leak ex.Message (có thể chứa schema DB / file path).
/// Development: kèm exception type + message để debug nhanh.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            logger.LogError(ex,
                "Unhandled exception. TraceId={TraceId}, Path={Path}, Method={Method}",
                traceId, context.Request.Path, context.Request.Method);

            // Nếu response đã bắt đầu gửi xuống client (SignalR, streaming, Swagger middleware
            // đã write header/body) thì không thể set status/header nữa — chỉ log và rethrow để
            // Kestrel đóng connection. Nếu cố set sẽ throw "Headers are read-only".
            if (context.Response.HasStarted)
            {
                logger.LogWarning(
                    "Response has already started — cannot write error payload. TraceId={TraceId}",
                    traceId);
                throw;
            }

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
    {
        context.Response.Clear();
        context.Response.ContentType = "application/json";

        ApiResponse response;

        // BusinessException — message là intent của developer, an toàn để trả về
        if (ex is Infr.Exceptions.BusinessException businessEx)
        {
            context.Response.StatusCode = (int)businessEx.StatusCode;
            response = ApiResponse.Fail(
                statusCode: (int)businessEx.StatusCode,
                code: businessEx.ErrorCode ?? "BUSINESS_ERROR",
                message: businessEx.Message);
            response.TraceId = traceId;
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response = ApiResponse.Fail(
                statusCode: 500,
                code: "INTERNAL",
                message: env.IsDevelopment()
                    ? $"{ex.GetType().Name}: {ex.Message}"
                    : "Internal server error. Vui lòng liên hệ hỗ trợ với mã traceId.");
            response.TraceId = traceId;
        }

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
    }
}
