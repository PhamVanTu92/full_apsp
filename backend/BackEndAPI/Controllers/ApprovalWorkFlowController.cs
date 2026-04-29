using System.Security.Claims;
using BackEndAPI.Data;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Service;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalWorkFlowController(
    IApprovalWorkFlowService approvalWorkFlowService,
    IHttpContextAccessor contextAccessor,
    AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] GridifyQuery gridifyQuery, [FromQuery] string? search)
    {
        var result = await approvalWorkFlowService.GetAllAsync(gridifyQuery, search);


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
        var result = await approvalWorkFlowService.GetByIdAsync(id);
        if (result.Item2 is null)
            return Ok(result.Item1);
        return Ok(result.Item2);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveAsync(CreateApprovalRequest approvalRequest)
    {
        var result = await approvalWorkFlowService.ApprovalAsync(approvalRequest);
        if (result.Item1 is false) return Ok(result.Item2);
        return Ok(result.Item1);
    }


    [HttpPost()]
    public async Task<IActionResult> CreateApprove(CreateApprovalWorkFlowDto dto)
    {
        var docId   = dto.DocId;
        var doctype = dto.DocType;


        var document = await context.ODOC.FirstOrDefaultAsync(x => x.Id == docId && x.Status == "DXL");
        if (document is null)
            return Ok(new Mess
            {
                Status = 400,
                Errors = "Chứng từ không thể duyệt"
            });

        var isExisted = await approvalWorkFlowService.CheckApprovalAsync(docId, doctype);
        var user      = contextAccessor.HttpContext?.User;
        var userId    = int.Parse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        if (isExisted.Count == 0)
            return Ok(new Mess
            {
                Status = 400,
                Errors = "Không tìm thấy mẫu phê duyệt phù hợp"
            });

        var result = await approvalWorkFlowService.CreateAsync(new List<IdAndTypeDocDto>()
        {
            new IdAndTypeDocDto
            {
                Id      = docId,
                Type    = doctype,
                DocType = document?.InvoiceCode ?? ""
            }
        }, userId, isExisted);
        return Ok(result.First());
    }
}