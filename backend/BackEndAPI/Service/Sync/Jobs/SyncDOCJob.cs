namespace BackEndAPI.Service.Sync.Jobs;

/// <summary>
/// Sync 3 luồng document từ proxy nội bộ:
///   1. ApprovalOrder (TTDH)
///   2. RejectOrder   (TTDHH)
///   3. CancelYCHG    (RejectIssue)
/// Proxy là blackbox — không delta phía client. Cron 2 giờ.
/// Wrapped qua IInternalProxySyncService cho checkpoint + log từng bước.
/// </summary>
public class SyncDOCJob : SyncJobBase
{
    private readonly IInternalProxySyncService _syncService;

    public SyncDOCJob(ILogger<SyncDOCJob> logger, IInternalProxySyncService syncService) : base(logger)
    {
        _syncService = syncService;
    }

    protected override async Task RunAsync(CancellationToken ct)
    {
        // 3 step độc lập — nếu 1 step fail (throw), 2 step sau bị skip.
        // Đó là behavior cũ. Nếu cần "isolate" mỗi step, đổi sang try/catch riêng.
        await _syncService.SyncApprovalOrderAsync(ct);
        await _syncService.SyncRejectOrderAsync(ct);
        await _syncService.SyncCancelYCHGAsync(ct);
    }
}
