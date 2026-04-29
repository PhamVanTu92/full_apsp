using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;

namespace BackEndAPI.Service.Notification;

public class WebSocketHandler(WebSocketService manager)
{
    private readonly WebSocketService _webSocketManager = manager;

    public async Task Handle(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            try
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                // Lấy Bearer Token từ Header
                var token = context.Request.Query["token"].ToString();

                // Trích xuất ID người dùng từ token
                var userId = GetUserIdFromToken(token);

                // Thêm socket vào manager
                if (string.IsNullOrEmpty(userId))
                    return;
                _webSocketManager.AddSocket(userId, webSocket);

                // Xử lý WebSocket (như lắng nghe tin nhắn và ngắt kết nối)
                await ProcessWebSocketAsync(webSocket, userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task ProcessWebSocketAsync(WebSocket webSocket, string userId)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = null;

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            // Xóa socket khỏi manager khi kết nối bị ngắt
            _webSocketManager.RemoveSocket(userId, webSocket);
        }
    }

    private string? GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "userId");
        return userIdClaim?.Value;
    }
}