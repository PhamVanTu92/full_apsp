namespace BackEndAPI.Service.Sync.Jobs;

/// <summary>
/// Push VPKM (ObjType=12) lên SAP. Đã refactor sang queue pattern (xem
/// <see cref="IDocumentPushSyncService.PushVPKMBatchAsync"/>) để tránh bug "1 doc/cycle".
/// Cron mặc định: mỗi 58 giây.
/// </summary>
public class SyncVPKMJob : SyncJobBase
{
    private readonly IDocumentPushSyncService _pushService;

    public SyncVPKMJob(ILogger<SyncVPKMJob> logger, IDocumentPushSyncService pushService) : base(logger)
    {
        _pushService = pushService;
    }

    protected override Task RunAsync(CancellationToken ct)
        => _pushService.PushVPKMBatchAsync(ct);
}
