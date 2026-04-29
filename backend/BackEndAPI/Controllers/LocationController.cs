using BackEndAPI.Middleware;
using BackEndAPI.Service.Cache;
using BackEndAPI.Service.LocationService;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace BackEndAPI.Controllers;

[ApiController]
public class LocationController(ILocaltion localtion, IReferenceDataCache cache) : Controller
{
    private readonly ILocaltion _localtion = localtion;
    private readonly IReferenceDataCache _cache = cache;
    private readonly ResponseClients _responseClients = new ResponseClients();

    [HttpGet("api/areas")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAreas([FromQuery] string? search, [FromQuery] GridifyQuery f,
        int skip = 0, int limit = 30)
    {
        var (items, total, mess) = await _localtion.GetAreas(skip, limit, f, search);
        return mess != null
            ? _responseClients.GetStatusCode(mess.Status, mess)
            : Ok(new { items, skip, limit, total });
    }

    /// <summary>
    /// Get regions — cached khi là full-list query (no search/filter động).
    /// Frontend thường gọi <c>/api/regions?skip=0&amp;limit=10000</c> → cache hit.
    /// </summary>
    [HttpGet("api/regions")]
    [AllowAnonymous]
    [CacheableReferenceData(ReferenceEntities.Region)]
    public async Task<IActionResult> GetRegion([FromQuery] string? search, [FromQuery] GridifyQuery f,
        int skip = 0, int limit = 30)
    {
        bool isCacheable = string.IsNullOrEmpty(search)
            && string.IsNullOrEmpty(f?.Filter)
            && string.IsNullOrEmpty(f?.OrderBy);

        if (isCacheable)
        {
            var cached = await _cache.GetOrSetAsync(
                $"{ReferenceEntities.Region}:{skip}:{limit}",
                async () =>
                {
                    var (data, total, mess) = await _localtion.GetRegions(skip, limit, f, search);
                    if (mess != null) throw new Exception(mess.Errors);
                    return new { items = data, skip, limit, total };
                });
            return Ok(cached);
        }

        var (itemsRaw, totalRaw, messRaw) = await _localtion.GetRegions(skip, limit, f, search);
        return messRaw != null
            ? _responseClients.GetStatusCode(messRaw.Status, messRaw)
            : Ok(new { items = itemsRaw, skip, limit, total = totalRaw });
    }
}
