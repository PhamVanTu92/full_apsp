using BackEndAPI.Data;
using BackEndAPI.Models.Sync;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.Sync.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BackEndAPI.Service.Sync;

public class InternalProxySyncService : IInternalProxySyncService
{
    private readonly IBusinessPartnerService _bpService;
    private readonly AppDbContext _db;
    private readonly ILogger<InternalProxySyncService> _logger;

    public InternalProxySyncService(
        IBusinessPartnerService bpService,
        AppDbContext db,
        ILogger<InternalProxySyncService> logger)
    {
        _bpService = bpService;
        _db = db;
        _logger = logger;
    }

    public Task<ProxySyncResult> SyncBPMasterDataAsync(CancellationToken ct)
        => RunAsync("SyncBPJob", _bpService.SyncBPCRD4Async, ct);

    public Task<ProxySyncResult> SyncApprovalOrderAsync(CancellationToken ct)
        => RunAsync("SyncDOCJob:Approval", _bpService.SyncTTDHAsync, ct);

    public Task<ProxySyncResult> SyncRejectOrderAsync(CancellationToken ct)
        => RunAsync("SyncDOCJob:Reject", _bpService.SyncTTDHHAsync, ct);

    public Task<ProxySyncResult> SyncCancelYCHGAsync(CancellationToken ct)
        => RunAsync("SyncDOCJob:CancelYCHG", _bpService.SyncCancelYCHGsync, ct);

    /// <summary>
    /// Wrap 1 method legacy (Func&lt;Task&lt;bool&gt;&gt;) với checkpoint + logging + cancellation.
    /// Method legacy không nhận CancellationToken — kiểm tra ct trước/sau call để fail-fast khi shutdown.
    /// </summary>
    private async Task<ProxySyncResult> RunAsync(string jobKey, Func<Task<bool>> legacyCall, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var checkpoint = await _db.SyncCheckpoints.FirstOrDefaultAsync(c => c.JobName == jobKey, ct);
        var sw = Stopwatch.StartNew();

        try
        {
            var ok = await legacyCall();
            sw.Stop();

            await UpdateCheckpointAsync(checkpoint, jobKey, ok ? "Success" : "Failed", (int)sw.ElapsedMilliseconds, error: null, ct);
            _logger.LogInformation("[{JobKey}] proxy sync done in {ElapsedMs}ms, returned {Result}",
                jobKey, sw.ElapsedMilliseconds, ok);

            return new ProxySyncResult { Success = ok, ElapsedMs = (int)sw.ElapsedMilliseconds };
        }
        catch (Exception ex)
        {
            sw.Stop();
            await UpdateCheckpointAsync(checkpoint, jobKey, "Failed", (int)sw.ElapsedMilliseconds,
                error: $"{ex.GetType().Name}: {ex.Message}", ct);
            throw;
        }
    }

    private async Task UpdateCheckpointAsync(SyncCheckpoint? existing, string jobKey, string status, int durationMs, string? error, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (existing == null)
        {
            _db.SyncCheckpoints.Add(new SyncCheckpoint
            {
                JobName = jobKey,
                LastSyncedAt = now,
                LastDurationMs = durationMs,
                LastStatus = status,
                LastError = error,
                UpdatedAt = now
            });
        }
        else
        {
            existing.LastSyncedAt = now;
            existing.LastDurationMs = durationMs;
            existing.LastStatus = status;
            existing.LastError = error;
            existing.UpdatedAt = now;
        }
        await _db.SaveChangesAsync(ct);
    }
}
