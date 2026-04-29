using BackEndAPI.Data;
using BackEndAPI.Models.Sync;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Sync.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BackEndAPI.Service.Sync;

public class DocumentPushSyncService : IDocumentPushSyncService
{
    private const string JobKey = "SyncJob";
    private const int BatchLimit = 50;
    private static readonly TimeSpan SoftDeadline = TimeSpan.FromMinutes(3);

    private readonly IDocumentService _docService;
    private readonly AppDbContext _db;
    private readonly ILogger<DocumentPushSyncService> _logger;

    public DocumentPushSyncService(
        IDocumentService docService,
        AppDbContext db,
        ILogger<DocumentPushSyncService> logger)
    {
        _docService = docService;
        _db = db;
        _logger = logger;
    }

    public async Task<PushBatchResult> PushPendingBatchAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var deadline = DateTime.UtcNow + SoftDeadline;
        var result = new PushBatchResult();

        result.DraftsPushed = await DrainAsync("draft",
            CountDraftsAsync,
            _docService.SyncToSapDraftAsync,
            ct, deadline, result);

        result.DocsPushed = await DrainAsync("doc",
            CountDocsAsync,
            _docService.SyncToSapAsync,
            ct, deadline, result);

        result.IssuesPushed = await DrainAsync("issue",
            CountIssuesAsync,
            _docService.SyncIssueToSapAsync,
            ct, deadline, result);

        result.RemainingDrafts = await CountDraftsAsync(ct);
        result.RemainingDocs = await CountDocsAsync(ct);
        result.RemainingIssues = await CountIssuesAsync(ct);

        sw.Stop();
        result.ElapsedMs = (int)sw.ElapsedMilliseconds;

        await UpsertCheckpointAsync(result, ct);

        _logger.LogInformation(
            "Push batch done: drafts={Drafts}, docs={Docs}, issues={Issues}, stuck={Stuck}, " +
            "remaining(drafts={RD}, docs={RDoc}, issues={RI}) in {ElapsedMs}ms",
            result.DraftsPushed, result.DocsPushed, result.IssuesPushed, result.StuckDocs,
            result.RemainingDrafts, result.RemainingDocs, result.RemainingIssues, result.ElapsedMs);

        return result;
    }

    /// <summary>
    /// Lặp gọi <paramref name="pushOne"/> cho đến khi: hết pending, đạt batch limit, deadline, hoặc cancellation.
    /// Sau mỗi push, đếm lại pending — nếu không giảm → stuck doc, log + break.
    /// </summary>
    private async Task<int> DrainAsync(
        string kind,
        Func<CancellationToken, Task<int>> pendingCount,
        Func<Task<bool>> pushOne,
        CancellationToken ct,
        DateTime deadline,
        PushBatchResult result)
    {
        int pushed = 0;
        for (int i = 0; i < BatchLimit; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (DateTime.UtcNow >= deadline)
            {
                _logger.LogInformation("Push {Kind}: soft deadline reached after {Pushed} items", kind, pushed);
                break;
            }

            var before = await pendingCount(ct);
            if (before == 0) break;

            await pushOne();

            var after = await pendingCount(ct);
            if (after >= before)
            {
                // Doc không được mark synced — nhiều khả năng push fail nội bộ (catch nuốt return true).
                result.StuckDocs++;
                _logger.LogWarning(
                    "Push {Kind} did not advance queue: was {Before}, now {After}. Aborting drain to avoid infinite loop.",
                    kind, before, after);
                break;
            }
            pushed += (before - after);
        }
        return pushed;
    }

    // Predicates phải khớp đúng với FirstOrDefault trong DocumentService.* (DocumentService.cs:1073, 240, 6895)

    public async Task<int> PushVPKMBatchAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var deadline = DateTime.UtcNow + SoftDeadline;
        var result = new PushBatchResult();

        var pushed = await DrainAsync("vpkm",
            ct1 => _db.ODOC.CountAsync(x => x.Status == "DXN" && x.ObjType == 12 && x.IsSync == false, ct1),
            _docService.SyncVPKMToSapAsync,
            ct, deadline, result);

        sw.Stop();
        _logger.LogInformation("VPKM push batch: {Pushed} pushed, {Stuck} stuck, in {Ms}ms",
            pushed, result.StuckDocs, sw.ElapsedMilliseconds);
        return pushed;
    }

    private Task<int> CountDraftsAsync(CancellationToken ct)
        => _db.ODOC.CountAsync(x => x.Status == "DXN" && x.ObjType == 22 && x.IsCheck == false && x.IsSync == true, ct);

    private Task<int> CountDocsAsync(CancellationToken ct)
        => _db.ODOC.CountAsync(x => x.Status == "DXN" && x.ObjType == 22 && x.IsSync == false, ct);

    private Task<int> CountIssuesAsync(CancellationToken ct)
        => _db.ODOC.CountAsync(x => x.Status == "DXN" && x.ObjType == 1250000001 && x.IsSync == false, ct);

    private async Task UpsertCheckpointAsync(PushBatchResult result, CancellationToken ct)
    {
        var existing = await _db.SyncCheckpoints.FirstOrDefaultAsync(c => c.JobName == JobKey, ct);
        var totalPushed = result.DraftsPushed + result.DocsPushed + result.IssuesPushed;
        var status = result.StuckDocs > 0 ? "PartialSuccess" : "Success";

        if (existing == null)
        {
            _db.SyncCheckpoints.Add(new SyncCheckpoint
            {
                JobName = JobKey,
                LastSyncedAt = DateTime.UtcNow,
                LastDurationMs = result.ElapsedMs,
                LastRecordsProcessed = totalPushed,
                LastStatus = status,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.LastSyncedAt = DateTime.UtcNow;
            existing.LastDurationMs = result.ElapsedMs;
            existing.LastRecordsProcessed = totalPushed;
            existing.LastStatus = status;
            existing.LastError = result.StuckDocs > 0
                ? $"{result.StuckDocs} stuck doc(s) — check legacy push methods"
                : null;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
    }
}
