using System.Security.Cryptography;
using System.Text;

namespace BackEndAPI.Middleware;

/// <summary>
/// HTTP ETag support cho cache reference data.
///
/// Hoạt động:
///   1. Response body được hash (SHA1) → ETag header.
///   2. Lần sau client gửi If-None-Match: &lt;hash&gt;.
///   3. Nếu match → response 304 Not Modified, body rỗng → tiết kiệm bandwidth.
///   4. Cache-Control: private, max-age=300 (5 phút) → browser tự cache.
///
/// Chỉ apply cho endpoint có header phản hồi <c>X-Cacheable: true</c>
/// (set bởi <see cref="CacheableReferenceDataAttribute"/>).
/// </summary>
public class ETagMiddleware
{
    private const string MarkerHeader = "X-Cacheable";

    private readonly RequestDelegate _next;
    private readonly ILogger<ETagMiddleware> _logger;

    public ETagMiddleware(RequestDelegate next, ILogger<ETagMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Chỉ apply cho GET — POST/PUT không cache
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var ms = new MemoryStream();
        context.Response.Body = ms;

        await _next(context);

        // Chỉ wrap khi controller đánh dấu cacheable (header X-Cacheable: true)
        if (!context.Response.Headers.ContainsKey(MarkerHeader)
            || context.Response.StatusCode != 200)
        {
            ms.Position = 0;
            await ms.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
            return;
        }

        // Bỏ marker header trước khi gửi response
        context.Response.Headers.Remove(MarkerHeader);

        ms.Position = 0;
        var etag = ComputeETag(ms);
        context.Response.Headers["ETag"] = etag;
        context.Response.Headers["Cache-Control"] = "private, max-age=300, must-revalidate";

        // If-None-Match match → 304
        var ifNoneMatch = context.Request.Headers["If-None-Match"].ToString();
        if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etag)
        {
            context.Response.StatusCode = StatusCodes.Status304NotModified;
            context.Response.ContentLength = 0;
            context.Response.Body = originalBody;
            _logger.LogDebug("ETag match → 304: {Path} {Etag}", context.Request.Path, etag);
            return;
        }

        // Trả body bình thường
        ms.Position = 0;
        await ms.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    private static string ComputeETag(Stream body)
    {
        body.Position = 0;
        using var sha = SHA1.Create();
        var hash = sha.ComputeHash(body);
        return $"\"{Convert.ToHexString(hash)[..16]}\"";
    }
}

/// <summary>
/// Action attribute đánh dấu endpoint hỗ trợ ETag caching.
/// Truyền entityType → ETag được tính từ cache version (stable theo data,
/// không bị ảnh hưởng bởi traceId/timestamp trong response envelope).
///
/// Cách dùng:
/// <code>
/// [HttpGet("getall")]
/// [CacheableReferenceData(ReferenceEntities.Brand)]
/// public async Task&lt;IActionResult&gt; GetAll() { ... }
/// </code>
/// </summary>
public class CacheableReferenceDataAttribute : Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute
{
    public string? EntityType { get; }

    public CacheableReferenceDataAttribute(string? entityType = null)
    {
        EntityType = entityType;
    }

    public override void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
    {
        if (context.HttpContext.Response.StatusCode != 200) return;

        if (!string.IsNullOrEmpty(EntityType))
        {
            // ETag từ cache version — stable cho cùng dataset.
            var cache = context.HttpContext.RequestServices
                .GetService(typeof(BackEndAPI.Service.Cache.IReferenceDataCache))
                as BackEndAPI.Service.Cache.IReferenceDataCache;
            if (cache != null)
            {
                var version = cache.GetVersion(EntityType);
                var etag = $"\"{EntityType}-v{version}\"";

                // Check If-None-Match → trả 304 ngay
                var ifNoneMatch = context.HttpContext.Request.Headers["If-None-Match"].ToString();
                if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etag)
                {
                    context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(304);
                    context.HttpContext.Response.Headers["ETag"] = etag;
                    context.HttpContext.Response.Headers["Cache-Control"] = "private, max-age=300, must-revalidate";
                    return;
                }

                context.HttpContext.Response.Headers["ETag"] = etag;
                context.HttpContext.Response.Headers["Cache-Control"] = "private, max-age=300, must-revalidate";
            }
        }

        base.OnActionExecuted(context);
    }
}
