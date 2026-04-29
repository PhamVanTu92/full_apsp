using BackEndAPI.Dtos;
using BackEndAPI.Service.Approval_V2.ApprovalLevel;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalLevelController(IApprovalLevelService approvalLevelService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] GridifyQuery gridifyQuery, [FromQuery] string? search)
    {
        var result = await approvalLevelService.GetAllAsync(gridifyQuery, search);


        return Ok(new Pagination
        {
            Result   = result.Item1,
            Page     = gridifyQuery.Page,
            PageSize = gridifyQuery.PageSize,
            Total    = result.Item2
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await approvalLevelService.GetByIdAsync(id);
        if (result.Item2 is null)
            return Ok(result.Item1);
        return Ok(result.Item2);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await approvalLevelService.DeleteAsync(id);

        if (result is not null) return Ok();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateApprovalLevel dto)
    {
        var result = await approvalLevelService.CreateAsync(dto);
        if (result.Item2 is null) return Ok(result.Item1);
        return Ok(result.Item2);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, CreateApprovalLevel dto)
    {
        var result = await approvalLevelService.UpdateAsync(id, dto);
        if (result.Item2 is null) return Ok(result.Item1);
        return Ok(result.Item2);
    }
}