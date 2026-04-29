namespace BackEndAPI.Service.Sync.Jobs;

/// <summary>
/// Sync BP master data từ proxy nội bộ về local DB.
/// Proxy là blackbox — không thể delta phía client. Sync full mỗi lần (cron 10 phút).
/// Đã wrap qua IInternalProxySyncService để có checkpoint + structured logging.
/// </summary>
public class SyncBPJob : SyncJobBase
{
    private readonly IInternalProxySyncService _syncService;

    public SyncBPJob(ILogger<SyncBPJob> logger, IInternalProxySyncService syncService) : base(logger)
    {
        _syncService = syncService;
    }

    protected override Task RunAsync(CancellationToken ct)
        => _syncService.SyncBPMasterDataAsync(ct);
}
