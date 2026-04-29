namespace BackEndAPI.Service.Sync.Jobs;

/// <summary>
/// Push docs từ local lên SAP — Drafts, regular Docs, Issues.
///
/// Trước refactor: gọi mỗi method 1 lần → mỗi cron tick chỉ push được 3 docs.
/// Sau refactor: queue pattern qua <see cref="IDocumentPushSyncService"/> — drain đến hết
/// pending hoặc đạt batch limit (50) / soft deadline (3 phút).
/// Có stuck-doc detection để tránh vòng lặp vô tận trên doc lỗi.
/// </summary>
public class SyncJob : SyncJobBase
{
    private readonly IDocumentPushSyncService _pushService;

    public SyncJob(ILogger<SyncJob> logger, IDocumentPushSyncService pushService) : base(logger)
    {
        _pushService = pushService;
    }

    protected override Task RunAsync(CancellationToken ct)
        => _pushService.PushPendingBatchAsync(ct);
}
