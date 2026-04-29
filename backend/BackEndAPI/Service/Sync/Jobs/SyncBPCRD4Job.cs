namespace BackEndAPI.Service.Sync.Jobs;

/// <summary>
/// Sync trạng thái thanh toán hoá đơn (CRD4) từ SAP.
/// Đã refactor sang delta sync (xem <see cref="IBPSyncService"/>):
/// 1 HTTP call / chu kỳ thay vì N call (N = số BP).
/// Cron mặc định: mỗi 30 giây.
/// </summary>
public class SyncBPCRD4Job : SyncJobBase
{
    private readonly IBPSyncService _syncService;

    public SyncBPCRD4Job(ILogger<SyncBPCRD4Job> logger, IBPSyncService syncService) : base(logger)
    {
        _syncService = syncService;
    }

    protected override Task RunAsync(CancellationToken ct)
        => _syncService.SyncCRD4DeltaAsync(ct);
}
