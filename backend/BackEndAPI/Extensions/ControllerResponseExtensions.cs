using BackEndAPI.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Extensions;

/// <summary>
/// Extension methods để controller trả ApiResponse&lt;T&gt; đồng nhất, ngắn gọn.
/// </summary>
public static class ControllerResponseExtensions
{
    public static IActionResult OkResponse<T>(this ControllerBase ctrl, T data, string? message = null)
        => ctrl.Ok(WithTraceId(ctrl, ApiResponse<T>.Ok(data, message)));

    public static IActionResult CreatedResponse<T>(this ControllerBase ctrl, T data, string? message = null)
        => ctrl.StatusCode(201, WithTraceId(ctrl, ApiResponse<T>.Created(data, message)));

    public static IActionResult NoContentResponse(this ControllerBase ctrl, string? message = null)
        => ctrl.StatusCode(200, WithTraceId(ctrl, ApiResponse.NoContent(message)));

    public static IActionResult NotFoundResponse(this ControllerBase ctrl, string message)
        => ctrl.StatusCode(404, WithTraceId(ctrl, ApiResponse.Fail(404, "NOT_FOUND", message)));

    public static IActionResult BadRequestResponse(this ControllerBase ctrl, string message, List<string>? errors = null)
        => ctrl.StatusCode(400, WithTraceId(ctrl, ApiResponse.Fail(400, "VALIDATION_ERROR", message, errors)));

    public static IActionResult BusinessErrorResponse(this ControllerBase ctrl, string code, string message, int statusCode = 400)
        => ctrl.StatusCode(statusCode, WithTraceId(ctrl, ApiResponse.Fail(statusCode, code, message)));

    public static IActionResult UnauthorizedResponse(this ControllerBase ctrl, string? message = null)
        => ctrl.StatusCode(401, WithTraceId(ctrl, ApiResponse.Fail(401, "UNAUTHORIZED", message ?? "Chưa đăng nhập")));

    public static IActionResult ForbiddenResponse(this ControllerBase ctrl, string? message = null)
        => ctrl.StatusCode(403, WithTraceId(ctrl, ApiResponse.Fail(403, "FORBIDDEN", message ?? "Không có quyền truy cập")));

    private static T WithTraceId<T>(ControllerBase ctrl, T resp) where T : ApiResponse<object>
    {
        resp.TraceId = ctrl.HttpContext?.TraceIdentifier;
        return resp;
    }

    // Generic version cho ApiResponse<T>
    private static ApiResponse<T> WithTraceId<T>(ControllerBase ctrl, ApiResponse<T> resp)
    {
        resp.TraceId = ctrl.HttpContext?.TraceIdentifier;
        return resp;
    }
}
