using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Notification(Service.Notification.Notification notficationService, IWebHostEnvironment webHostEnvironment)
    : Controller
{
    private readonly Service.Notification.Notification _notficationService = notficationService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly ResponseClients _responseClients = new ResponseClients();

    [HttpGet]
    public async Task<IActionResult> GetUserNotification(int skip = 0, int limit = 100)
    {
        
            var userClaims = User.Claims.ToList();
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var ok = int.TryParse(userId.ToString(), out var iUserId);

        var (data, mess, total, readCount) = await _notficationService.GetUserNotifications(iUserId,skip, limit);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(new {skip, limit, data, total,
            readTotal = readCount});
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> ReadNotification(int id)
    {
        var mess = await _notficationService.ChangeStatusNotification(id, true);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }
    [HttpPost("{id:int}/unread")]
    public async Task<IActionResult> UnreadNotification(int id)
    {
        var mess = await _notficationService.ChangeStatusNotification(id, false);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

}
