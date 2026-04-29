using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BackEndAPI.Service.Notification;

public class WebSocketService
{
    private readonly ConcurrentDictionary<string, List<WebSocket>> _sockets =
        new ConcurrentDictionary<string, List<WebSocket>>();

    public void AddSocket(string userId, WebSocket socket)
    {
        if (!_sockets.ContainsKey(userId))
        {
            _sockets[userId] = new List<WebSocket>();
        }

        _sockets[userId].Add(socket);
    }

    public void RemoveSocket(string userId, WebSocket socket)
    {
        if (_sockets.ContainsKey(userId))
        {
            _sockets[userId].Remove(socket);
            if (_sockets[userId].Count == 0)
            {
                _sockets.TryRemove(userId, out _);
            }
        }
    }

    public List<WebSocket>? GetSockets(string userId)
    {
        _sockets.TryGetValue(userId, out var sockets);
        return sockets;
    }

    public IEnumerable<WebSocket> GetAllSockets()
    {
        return _sockets.Values.SelectMany(s => s);
    }

    public async Task SendMessageToUsers(List<string> userIds, object message)
    {
        try
        {
            foreach (var userId in userIds)
            {
                if (_sockets.TryGetValue(userId, out var sockets))
                {
                    foreach (var socket in sockets)
                    {
                        if (socket.State == WebSocketState.Open)
                        {
                            var messJson = System.Text.Json.JsonSerializer.Serialize(message);
                            var buffer = Encoding.UTF8.GetBytes(messJson);
                            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                                CancellationToken.None);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}