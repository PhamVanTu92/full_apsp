using BackEndAPI.Service.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(DashboardService dashboardService) : Controller
{
    private readonly DashboardService _dashboardService = dashboardService;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAdminDashboard(int? month, bool isInterCom = false, DateTime? FromDate =null, DateTime? ToDate = null)
    {
        var (data, mess) = await _dashboardService.GetAdminDashboard(month, isInterCom, FromDate, ToDate);
        if (mess is not null)
        {
            return BadRequest(mess);
        }

        return Ok(data);
    }

    [Authorize]
    [HttpGet("customer")]
    public async Task<IActionResult> GetCustomerDashboard()
    {
        var claims = User.Claims.ToList();

        var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
        if (cardId == null)
        {
            return Unauthorized();
        }

        var ok = Int32.TryParse(cardId.ToString(), out int iCardId);
        if (!ok) return Unauthorized();
        
        var (data, mess) = await _dashboardService.GetCustomerDashboard(iCardId);
        if (mess is not null)
        {
            return BadRequest(mess);
        }

        return Ok(data);
    }
}