using BackEndAPI.Dtos;
using BackEndAPI.Service.Approval_V2.ApprovalSample;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalSampleController(IApprovalSampleService approvalSampleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] GridifyQuery gridifyQuery, [FromQuery] string? search)
    {
        var result = await approvalSampleService.GetAllAsync(gridifyQuery, search);


        return Ok(new Pagination()
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
        var result = await approvalSampleService.GetByIdAsync(id);
        if (result.Item2 is null)
            return Ok(result.Item1);
        return Ok(result.Item2);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await approvalSampleService.DeleteAsync(id);

        if (result is not null) return Ok();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateApprovalSample dto)
    {
        var result = await approvalSampleService.CreateAsync(dto);
        if (result.Item2 is null) return Ok(result.Item1);
        return Ok(result.Item2);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, CreateApprovalSample dto)
    {
        var result = await approvalSampleService.UpdateAsync(id, dto);
        if (result.Item2 is null) return Ok(result.Item1);
        return Ok(result.Item2);
    }
}