namespace BackEndAPI.Middleware;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LogUserActivityAttribute(string name) : Attribute
{
    public string Name { get; init; }
}

public class UserActivityLoggingMiddleware
{
    private readonly RequestDelegate                        _next;
    private readonly ILogger<UserActivityLoggingMiddleware> _logger;

    public UserActivityLoggingMiddleware(RequestDelegate next, ILogger<UserActivityLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var request   = context.Request;
        var user      = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var path      = request.Path;
        var method    = request.Method;

        // Optional: đọc body nếu là POST/PUT/PATCH
        string requestBody = "";
        if (request.ContentLength > 0 && request.Body.CanSeek)
        {
            request.EnableBuffering();
            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);
        }

        _logger.LogInformation(
            "[UserActivity] {Time} | {User} | {IP} | {Method} {Path} | Agent: {UserAgent} | Body: {Body}",
            DateTime.UtcNow,
            user,
            ipAddress,
            method,
            path,
            userAgent,
            requestBody
        );

        await _next(context); // tiếp tục chuỗi middleware
    }
}