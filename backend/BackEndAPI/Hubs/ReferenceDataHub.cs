using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BackEndAPI.Hubs;

/// <summary>
/// SignalR hub broadcast event khi reference data thay đổi.
/// Client subscribe vào event "ReferenceDataChanged" để biết invalidate local cache.
///
/// Connect URL: <c>/api/hubs/reference-data</c>
///
/// Client JavaScript example:
/// <code>
/// const hub = new signalR.HubConnectionBuilder()
///   .withUrl("/api/hubs/reference-data", { accessTokenFactory: () => token })
///   .withAutomaticReconnect()
///   .build();
///
/// hub.on("ReferenceDataChanged", (e) => {
///   // e: { entityType: "brand", version: 5, timestamp: "..." }
///   localCache.invalidate(e.entityType);
///   reloadBrands();
/// });
/// </code>
/// </summary>
[Authorize]
public class ReferenceDataHub : Hub
{
    private readonly ILogger<ReferenceDataHub> _logger;

    public ReferenceDataHub(ILogger<ReferenceDataHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("ReferenceDataHub connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("ReferenceDataHub disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
