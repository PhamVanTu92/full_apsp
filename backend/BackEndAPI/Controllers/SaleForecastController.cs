using System.Security.Claims;
using BackEndAPI.Models.SaleForecastModel;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.SaleForecast;
using Gridify;
using Microsoft.AspNetCore.Mvc;
using SaleForecast = BackEndAPI.Models.SaleForecastModel.SaleForecast;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/sale-forecast")]
public class SaleForecastController(ISaleForecast saleForecast) : Controller
{
   private readonly ISaleForecast _saleForecast = saleForecast;
    private readonly ResponseClients _responseClients = new ResponseClients();

    
    [PrivilegeRequirement("SaleForecast.View")]
    [HttpGet]
    public async Task<IActionResult> GetAllSaleForecast([FromQuery] string? search, [FromQuery] GridifyQuery f,
        int skip = 0, int limit = 30)
    {
        var (saleForecast, mess, total) = await _saleForecast.GetSaleForecast(skip, limit, search, f, 0);
        return mess != null
            ? _responseClients.GetStatusCode(mess.Status, mess)
            : Ok(new { plans = saleForecast, skip, limit, total });
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetAllSaleForecastMe([FromQuery] string? search, [FromQuery] GridifyQuery f,
        int skip = 0, int limit = 30)
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
        var (saleForecast, mess, total) = await _saleForecast.GetSaleForecast(skip, limit, search, f, cardId);
        return mess != null
            ? _responseClients.GetStatusCode(mess.Status, mess)
            : Ok(new { plans = saleForecast, skip, limit, total });
    }


    [PrivilegeRequirement("SaleForecast.View")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBySaleForecastId(int id)
    {
        var (saleForecast, mess) = await _saleForecast.GetSaleForecastById(id);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(saleForecast);
    }

    [PrivilegeRequirement("SaleForecast.Create")]
    [HttpPost]
    public async Task<IActionResult> CreateSaleForecast([FromBody] SaleForecast saleForecast)
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

        saleForecast.UserId = intUserId;

        var (newSaleForecast, mess) = await _saleForecast.CreateSaleForecast(saleForecast);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(newSaleForecast);
    }

    [PrivilegeRequirement("SaleForecast.Edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderPlan(int id, [FromBody] SaleForecast saleForecast)
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

        var (plan, mess) = await _saleForecast.UpdateSaleForecast(id, saleForecast, intUserId);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(plan);
    }

    
    [PrivilegeRequirement("SaleForecast.Edit")]
    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddSaleForecastItems(int id, [FromBody] List<SaleForecastItem> saleForecastItems)
    {
        var (saleForecast, mess) = await _saleForecast.AddItemToSaleForecast(id, saleForecastItems);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(saleForecast);
    }

    [PrivilegeRequirement("SaleForecast.Edit")]
    [HttpDelete("{id}/items")]
    public async Task<IActionResult> RemoveForecastItem(int id, List<int> itemIds)
    {
        var (saleForecast, mess) = await _saleForecast.RemoveItemFromSaleForecast(id, itemIds);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok(saleForecast);
    }


    [PrivilegeRequirement("SaleForecast.Confirm")]
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userType = claims.FindFirst("userType")?.Value;
        Console.WriteLine(userType);

        var mess = await _saleForecast.UpdateStatusSaleForecast(id, "A", userType);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok();
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var claims = User.Identity as ClaimsIdentity;
        if (claims == null)
        {
            return Unauthorized();
        }

        var userType = claims.FindFirst("userType")?.Value;
        Console.WriteLine(userType);

        var mess = await _saleForecast.UpdateStatusSaleForecast(id, "R", userType);
        return mess != null ? _responseClients.GetStatusCode(mess.Status, mess) : Ok();
    }
}