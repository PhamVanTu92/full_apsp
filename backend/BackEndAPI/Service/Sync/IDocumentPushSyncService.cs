using BackEndAPI.Service.Sync.Models;

namespace BackEndAPI.Service.Sync;

/// <summary>
/// Push docs từ local lên SAP qua queue pattern. Code legacy (SyncToSapDraftAsync/SyncToSapAsync/SyncIssueToSapAsync)
/// chỉ push 1 doc/lần gọi → backlog không clear kịp. Service này lặp gọi nhiều lần / chu kỳ cho đến khi:
///   • Hết doc pending,
///   • Đạt batch limit,
///   • CancellationToken trigger,
///   • Soft deadline (timeout) đạt.
/// Phát hiện stuck doc (push không advance queue) để tránh vòng lặp vô tận trên doc lỗi.
/// </summary>
public interface IDocumentPushSyncService
{
    Task<PushBatchResult> PushPendingBatchAsync(CancellationToken ct);

    /// <summary>Push VPKM batch (ObjType=12) — bug "1 doc/cycle" giống PushPendingBatchAsync.</summary>
    Task<int> PushVPKMBatchAsync(CancellationToken ct);
}

public class PushBatchResult
{
    public int DraftsPushed { get; set; }
    public int DocsPushed { get; set; }
    public int IssuesPushed { get; set; }
    public int StuckDocs { get; set; }
    public int ElapsedMs { get; set; }
    public int RemainingDrafts { get; set; }
    public int RemainingDocs { get; set; }
    public int RemainingIssues { get; set; }
}
