namespace BackEndAPI.Service.Sync.Jobs;

/// <summary>
/// Sync công nợ "current account balance" (CardCode-CRD4) từ SAP về local.
/// Đã refactor sang delta sync: query BPs có UpdateDate >= checkpoint thay vì foreach toàn bộ.
/// Cron mặc định: mỗi 2 giờ.
/// </summary>
public class SyncDebJob : SyncJobBase
{
    private readonly IBPSyncService _syncService;

    public SyncDebJob(ILogger<SyncDebJob> logger, IBPSyncService syncService) : base(logger)
    {
        _syncService = syncService;
    }

    protected override Task RunAsync(CancellationToken ct)
        => _syncService.SyncCardBalanceCRD4DeltaAsync(ct);
}
