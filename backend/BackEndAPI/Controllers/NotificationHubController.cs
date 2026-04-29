using BackEndAPI.Service.NotificationHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationHubController : Controller
    {
        private readonly IHubContext<NotificationHubs> _hubContext;

        public NotificationHubController(IHubContext<NotificationHubs> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotificationToAll([FromBody] string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            return Ok(new { Message = "Notification sent to all clients." });
        }
    }
}
