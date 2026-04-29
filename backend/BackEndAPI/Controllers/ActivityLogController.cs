using BackEndAPI.Data;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityLogController (AppDbContext context): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLog([FromQuery] GridifyQuery gQuery, [FromQuery] string? search)
    {
        var query = context.SystemLogs.AsQueryable().AsNoTracking().ApplyFiltering(gQuery);
        if (search is not null)
        {
            query = query.Where(e =>
                e.ActorName.Contains(search));
        }
        var total = await query.CountAsync();
        var items = await query.ApplyOrderingAndPaging(gQuery).ToListAsync();

        return Ok(new
        {
            items = items,
            total = total,
            page = gQuery.Page,
            pageSize = gQuery.PageSize
        });
    }
}