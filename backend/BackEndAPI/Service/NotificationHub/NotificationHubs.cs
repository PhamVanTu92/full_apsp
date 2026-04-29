using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace BackEndAPI.Service.NotificationHub
{
    public class NotificationHubs :  Hub
    {
        public async Task SendNotificationToAllAsync(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        public async Task SendNotificationToUserAsync(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // Đảm bảo UserId được gán đúng
            Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }
    }
}
