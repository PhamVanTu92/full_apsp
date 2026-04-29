using System.Runtime.InteropServices;
using System.Security.Claims;
using BackEndAPI.Models.Document;
using BackEndAPI.Service.Privile;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Commited(Service.Committeds.ICommittedService service)
    : Controller
{
    private readonly ResponseClients _responseClients = new ResponseClients();

    [PrivilegeRequirement("Committed.Create")]
    [HttpPost]
    public async Task<IActionResult> Create(Models.Committed.Committed cm)
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

        cm.UserId = intUserId;

        var (newPro, mess) = await service.CreateCommited(cm);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(newPro);
    }

    [PrivilegeRequirement("Committed.View")]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GridifyQuery q, string? search, int skip = 0, int limit = 30)
    {
        var (data, total, mess) = await service.GetCommitted(skip, limit, search, q, 0);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(new { data, total, skip, limit });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe([FromQuery] GridifyQuery q, string? search, int skip = 0, int limit = 30)
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
            return Unauthorized();
        }

        var (data, total, mess) = await service.GetCommitted(skip, limit, search, q, cardId);

        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(new { data, total, skip, limit });
    }

    [PrivilegeRequirement("Committed.View")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var (data, mess) = await service.GetCommitedById(id);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(data);
    }

    [PrivilegeRequirement("Committed.Edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Models.Committed.Committed cm)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userType = claims.FindFirst("userType")?.Value;

        cm.UserType = userType;

        var (data, mess) = await service.UpdateCommitted(id, cm);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(data);
    }

    [PrivilegeRequirement("Committed.Confirm")]
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm([FromRoute] int id)
    {
        var mess = await service.UpdateStatus(id, "A", "");
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

    [HttpPost("checking")]
    public async Task<IActionResult> CheckCommited([FromQuery] int cardId, [FromBody] List<ItemChecking> items)
    {
        var cmm = await service.GetCommitedDiscount(cardId, items);

        return Ok(cmm);
    }

    [PrivilegeRequirement("Committed.Reject")]
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] BodyRejectReason b)
    {
        var mess = await service.UpdateStatus(id, "R", b.RejectReason);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

    [PrivilegeRequirement("Committed.Cancel")]
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        var mess = await service.UpdateStatus(id, "C", "");
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

    [PrivilegeRequirement("Committed.Delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userType = claims.FindFirst("userType")?.Value;
        if (userType != "APSP") return Unauthorized();

        var mess = await service.DeleteCommited(id);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

    public class BodyRejectReason
    {
        public string RejectReason { get; set; }
    }
}