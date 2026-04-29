using BackEndAPI.Service.Sync.Models;

namespace BackEndAPI.Service.Sync;

/// <summary>
/// Wrapper cho các sync job dùng proxy nội bộ (không phải SAP B1 trực tiếp).
/// Proxy là blackbox với endpoint cố định — không thể delta từ phía client.
/// Các method ở đây chỉ thêm checkpoint tracking, structured logging, cancellation
/// và **không** thay đổi cách gọi proxy.
/// </summary>
public interface IInternalProxySyncService
{
    /// <summary>Sync BP master data từ proxy /BPMasterData. Dùng cho SyncBPJob.</summary>
    Task<ProxySyncResult> SyncBPMasterDataAsync(CancellationToken ct);

    /// <summary>Sync TTDH (approval order) từ proxy /ApprovalOrder. Dùng cho SyncDOCJob.</summary>
    Task<ProxySyncResult> SyncApprovalOrderAsync(CancellationToken ct);

    /// <summary>Sync TTDHH (reject order) từ proxy /RejectOrder. Dùng cho SyncDOCJob.</summary>
    Task<ProxySyncResult> SyncRejectOrderAsync(CancellationToken ct);

    /// <summary>Sync cancel YCHG (reject issue) từ proxy /RejectIssue. Dùng cho SyncDOCJob.</summary>
    Task<ProxySyncResult> SyncCancelYCHGAsync(CancellationToken ct);
}

public class ProxySyncResult
{
    public bool Success { get; set; }
    public int ElapsedMs { get; set; }
    public string? Note { get; set; }
}
