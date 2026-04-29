using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BackEndAPI.Models;
using BackEndAPI.Models.Account;
using BackEndAPI.Service.DebtReconciliation;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebtReconciliationController(IDebtReconcilicationService service) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Create(Models.DebtReconciliation data)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ok = Int32.TryParse(userId, out int intUserId);
        if (!ok || intUserId == 0)
        {
            return Unauthorized();
        }

        data.UserId = intUserId;

        var result = await service.CreateDebtReconciliation(data);
        return Ok(result);
    }

    [HttpPost("{id}/attachments")]
    public async Task<IActionResult> AddAtt([FromRoute] int id, [FromForm] List<IFormFile> files)
    {
        var result = await service.AddAttachmentToDebtReconciliation(id, files);
        return Ok(result);
    }

    [HttpPost("{id}/bp-attachments")]
    public async Task<IActionResult> AdBpAtt([FromRoute] int id, [FromForm] List<IFormFile> files)
    {
        var result = await service.AddAttachmentToDebtReconciliation(id, files, "bp");
        return Ok(result);
    }

    [HttpDelete("{id}/attachments")]
    public async Task<IActionResult> RemoveAtt([FromRoute] int id, [FromBody] List<int> ids)
    {
        var result = await service.RemoveAttachmentToDebtReconciliation(id, ids);
        return Ok(result);
    }

    [HttpPut("{id}/change-status/{status}")]
    public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromRoute] string status,
        [FromQuery] string? rejectReason)
    {
        var result = await service.ChangeStatusDebtReconiliation(id, status, rejectReason);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, Models.DebtReconciliation data)
    {
        var result = await service.UpdateDebtReconiliation(id, data);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? search, [FromQuery] GridifyQuery query)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userId = claims.FindFirst("cardId")?.Value;
        var ok = Int32.TryParse(userId, out int cardId);
        if (!ok || cardId == 0)
        {
            cardId = 0;
        }

        var (result, total) = await service.GetDebtReconciliations(search, query, cardId);

        return Ok(new
        {
            items = result,
            total = total,
            page = query.Page,
            pageSize = query.PageSize,
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.DebtReconciliation))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetDebtReconciliationById(id);

        return Ok(result);
    }

    public class CreateRequest
    {
        [JsonPropertyName("debt")] public string DebtReconciliation { get; set; } = string.Empty;
        [JsonPropertyName("files")] public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}