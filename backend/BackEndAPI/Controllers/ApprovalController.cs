using BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Service;
using BackEndAPI.Service.Approval;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalController(
    Approval approvalService,
    IWebHostEnvironment webHostEnvironment,
    IApprovalWorkFlowService approvalWorkFlowService)
    : Controller
{
    private readonly Service.Approval.Approval _approvalService         = approvalService;
    private readonly IWebHostEnvironment       _webHostEnvironment      = webHostEnvironment;
    private readonly ResponseClients           _responseClients         = new ResponseClients();
    private readonly IApprovalWorkFlowService  _approvalWorkFlowService = approvalWorkFlowService;

    // [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> GetApprovals([FromQuery] GridifyQuery q, string? status, string? search,
        int skip = 0, int limit = 100)
    {
        var (data, mess, total) = await _approvalService.GetApprovals(skip, limit, status, search, q);
        if (mess is not null)
        {
            return BadRequest(mess);
        }

        return Ok(new { skip, limit, data, total });
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetApprovalById(int id)
    {
        var (data, mess) = await _approvalService.GetApprovalById(id);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(data);
    }

    // [HttpPost]
    // public async Task<IActionResult> Approve(int docId, string status, string note = "")
    // {
    //     var result = await _approvalWorkFlowService.CheckApprovalAsync(docId,);
    // }

    [HttpPost]
    public async Task<IActionResult> Approve(int docId, string status, string note = "")
    {
        var claims = User.Claims.ToList();
    
        var userId = claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
    
        var ok = int.TryParse(userId.ToString(), out var iUserId);
        if (!ok)
        {
            return Unauthorized();
        }
    
        var mess = await _approvalService.Approve(docId, iUserId, status, note);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok("Duyệt thành công");
    }

    [AllowAnonymous]
    [HttpPost("action-purchase/{id:int}")]
    public async Task<IActionResult> ApproveAction(int id)
    {
        var (app, mess) = await _approvalService.ActionApproval(id);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(app);
    }
}