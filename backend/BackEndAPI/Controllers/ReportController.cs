using BackEndAPI.Service.ItemMasterData;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Report;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackEndAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportController(ReportService reportService) : Controller
{
    private readonly ReportService _reportService = reportService;

    [HttpGet("purchases")]
    [PrivilegeRequirement("Report.PurchaseItemReport")]
    public async Task<IActionResult> GetPurchases(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate
    )
    {
        if (startDate == null )
        {
            startDate = new DateTime(DateTime.Now.Year, 1, 1);
        }

        if (endDate == null)
        {
            endDate = DateTime.Today;
        }
        var claims = User.Claims.ToList();

        var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
     

        var ok = Int32.TryParse(cardId?.ToString() ?? "0", out int iCardId);

        var r = await _reportService.GetPurchaseReport(startDate.Value, endDate.Value, iCardId);

        return Ok(r);
    }

    [HttpGet("order-state")]
    [PrivilegeRequirement("Report.PurchaseOrderReport")]
    public async Task<IActionResult> GetPurchasesState([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, bool? isContern, [FromQuery] GridifyQuery query)
    {
        if (startDate == null)
        {
            startDate = new DateTime(DateTime.Now.Year, 1, 1);
        }

        if (endDate == null)
        {
            endDate = DateTime.Today;
        }
        var claims = User.Claims.ToList();

        var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;


        var ok = Int32.TryParse(cardId?.ToString() ?? "0", out int iCardId);

        var (order, item, total, totalBeforeVat, bonusAmount, bonusCommited, totalAfterVat, mes) = await _reportService.GetOrderState(iCardId, startDate.Value, endDate.Value, isContern, query);
        if (mes != null)
            return BadRequest(new { mes.Status, mes.Errors });
        return Ok(new { order , item, totalBeforeVat, bonusAmount, bonusCommited, totalAfterVat, total });
    }

    [HttpGet("purchases/{itemCode}")]
    [PrivilegeRequirement("Report.PurchaseItemReport")]
    public async Task<IActionResult> GetPurchasesDetail(
        [FromRoute] string itemCode, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate
    )
    {
        if (startDate == null )
        {
            startDate = new DateTime(DateTime.Now.Year, 1, 1);
        }

        if (endDate == null)
        {
            endDate = DateTime.Today;
        }        var claims = User.Claims.ToList();

        var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;

        var ok = Int32.TryParse(cardId?.ToString() ?? "0", out int iCardId);

        var r = await _reportService.GetItemPurchaseReport(startDate.Value, endDate.Value,itemCode, iCardId);

        return Ok(r);
    }
    [HttpGet("purchases/top/{top}")]
    [PrivilegeRequirement("Report.PurchaseItemReport")]
    public async Task<IActionResult> GetPurchasesTop(
        [FromRoute] int top, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate
    )
    {
        if (startDate == null )
        {
            startDate = new DateTime(DateTime.Now.Year, 1, 1);
        }

        if (endDate == null)
        {
            endDate = DateTime.Today;
        }
        var claims = User.Claims.ToList();

        var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;

        var ok = Int32.TryParse(cardId?.ToString() ?? "0", out int iCardId);

        var r = await _reportService.TopItemReport(startDate.Value, endDate.Value,iCardId, top);

        return Ok(r);
    }
    [HttpGet("InventoryReport")]
    [PrivilegeRequirement("Report.InventoryReport")]
    public async Task<IActionResult> InventoryReport( DateTime FromDate, DateTime ToDate, string? CardCode)
    {

        var (r,mess) = await _reportService.GetInventoryReport(FromDate, ToDate, CardCode);
        if (mess != null)
        {
            return BadRequest(new { mess.Status, mess.Errors });
        }
        return Ok(r);
    }
    [HttpGet("BalanceBPReport")]
    [PrivilegeRequirement("Report.DebtDetail")]
    public async Task<IActionResult> BalanceBPReport(DateTime FromDate, DateTime ToDate, string? CardCode)
    {

        var (r, mess) = await _reportService.GetBalanceBPReport(FromDate, ToDate, CardCode);
        if (mess != null)
        {
            return BadRequest(new { mess.Status, mess.Errors });
        }
        return Ok(r);
    }
    [HttpGet("debtReport")]
    [PrivilegeRequirement("Report.DebtReport")]
    public async Task<IActionResult> debtReport(DateTime ToDate, string? CardCode,string? Employee, string? location)
    {

        var (r, mess) = await _reportService.GetdebtBP(ToDate, CardCode, Employee,location);
        if (mess != null)
        {
            return BadRequest(new { mess.Status, mess.Errors });
        }
        return Ok(r);
    }
    [Authorize]
    [HttpGet("linkInvoice")]
    public async Task<IActionResult> linkInvoice(DateTime fromDate,DateTime ToDate)
    {

        var (r, mess) = await _reportService.GetLinkInovie(fromDate, ToDate);
        if (mess != null)
        {
            return BadRequest(new { mess.Status, mess.Errors });
        }
        return Ok(r);
    }
    [Authorize]
    [HttpGet("paynow")]
    [PrivilegeRequirement("Report.PolicyPayNow")]
    public async Task<IActionResult> getPayNow(DateTime fromDate, DateTime ToDate, string? CardId)
    {

        var claims = User.Claims.ToList();

        var cardId = claims.FirstOrDefault(c => c.Type == "cardId")?.Value;
        var (r, mess) = await _reportService.GetPayNow(fromDate, ToDate, CardId ?? cardId);
        if (mess != null)
        {
            return BadRequest(new { mess.Status, mess.Errors });
        }
        return Ok(r);
    }
    [Authorize]
    [HttpGet("zalo")]
    public async Task<IActionResult> getPayNow([FromQuery] GridifyQuery query)
    {
        var (r, mess, total) = await _reportService.GetZalo(query);
        if (mess != null)
        {
            return BadRequest(new { mess.Status, mess.Errors });
        }
        return Ok(new { zalo = r, total });
    }
}