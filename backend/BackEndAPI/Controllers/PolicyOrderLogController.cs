using BackEndAPI.Data;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PolicyOrderLogController (AppDbContext context): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLog([FromQuery] GridifyQuery gQuery, [FromQuery] string? search)
    {
        var query = context.PolicyOrderLogs.AsQueryable().AsNoTracking().ApplyFiltering(gQuery);
        if (search is not null)
        {
            query = query.Where(e =>
                search.Contains(e.CardCode) || search.Contains(e.CardName) || search.Contains(e.OrderCode));
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