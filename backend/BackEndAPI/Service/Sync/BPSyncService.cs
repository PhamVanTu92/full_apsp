using B1SLayer;
using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Sync;
using BackEndAPI.Service.Sync.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Sync;

/// <summary>
/// Delta sync CRD4 từ SAP B1 — query "Invoices có UpdateDate >= checkpoint" thay vì
/// duyệt từng BP. 1 HTTP call (+ pages) / chu kỳ thay vì N calls.
/// </summary>
public class BPSyncService : IBPSyncService
{
    private const string JobKeyInvoice = "SyncBPCRD4Job";
    private const string JobKeyBalance = "SyncDebJob";
    private const int SapPageSize = 100;

    /// <summary>Re-fetch thêm 1 ngày trước checkpoint để bù cho UpdateTime chưa track.</summary>
    private static readonly TimeSpan SafetyOverlap = TimeSpan.FromDays(1);

    private readonly SLConnection _sl;
    private readonly AppDbContext _db;
    private readonly ILogger<BPSyncService> _logger;

    public BPSyncService(SLConnection sl, AppDbContext db, ILogger<BPSyncService> logger)
    {
        _sl = sl;
        _db = db;
        _logger = logger;
    }

    public async Task<SyncResult> SyncCRD4DeltaAsync(CancellationToken ct)
    {
        var checkpoint = await GetCheckpointAsync(ct);
        var sinceUtc = (checkpoint?.LastSyncedAt ?? DateTime.UtcNow.AddDays(-30)) - SafetyOverlap;
        var since = sinceUtc.ToString("yyyy-MM-ddTHH:mm:ss");

        _logger.LogInformation("Delta sync CRD4 since {Since}", since);

        // 1. Một query SAP duy nhất (auto-pagination qua GetAllAsync) cho tất cả invoices đã update.
        var filter = $"UpdateDate ge '{since}' and (U_HTTT1 eq '2' or U_HTTT1 eq '3') and DocTotal gt 0";

        var sapItems = await _sl.Request("Invoices")
            .Filter(filter)
            .Select("DocEntry,CardCode,U_MDHPT,PaidToDate,DocTotal,DocDate,DocDueDate,U_HTTT1,UpdateDate")
            .OrderBy("UpdateDate")
            .WithPageSize(SapPageSize)
            .GetAllAsync<CRD4DeltaItem>();

        var result = new SyncResult { TotalFromSap = sapItems.Count };

        if (sapItems.Count == 0)
        {
            await UpdateCheckpointAsync(checkpoint, DateTime.UtcNow, result, ct);
            return result;
        }

        // 2. Map CardCode → BPId trong 1 query duy nhất (tránh N+1).
        var cardCodes = sapItems.Select(i => i.CardCode).Distinct().ToArray();
        var bpMap = await _db.BP
            .Where(b => cardCodes.Contains(b.CardCode))
            .Select(b => new { b.Id, b.CardCode })
            .ToDictionaryAsync(x => x.CardCode!, x => x.Id, ct);

        // 3. Lấy CRD4 hiện có theo InvoiceNumber để upsert (1 query batch).
        var invoiceNumbers = sapItems
            .SelectMany(i => new[] { i.DocEntry.ToString(), i.U_MDHPT })
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .ToArray();

        var existingMap = await _db.CRD4
            .Where(c => invoiceNumbers.Contains(c.InvoiceNumber))
            .ToDictionaryAsync(c => c.InvoiceNumber, ct);

        // 4. Group invoices theo InvoiceNumber để cộng dồn DocTotal/PaidToDate (giữ logic cũ).
        var grouped = sapItems
            .Where(i => bpMap.ContainsKey(i.CardCode))
            .GroupBy(i => !string.IsNullOrEmpty(i.U_MDHPT) ? i.U_MDHPT! : i.DocEntry.ToString());

        var toAdd = new List<CRD4>();
        foreach (var group in grouped)
        {
            ct.ThrowIfCancellationRequested();
            var invoiceNumber = group.Key;
            var first = group.First();
            var bpId = bpMap[first.CardCode];

            var totalAmount = group.Sum(g => g.DocTotal);
            var paidAmount = group.Sum(g => g.PaidToDate);
            var paymentMethod = group.Max(g => g.U_HTTT1);

            if (existingMap.TryGetValue(invoiceNumber, out var existing))
            {
                existing.BPId = bpId;
                existing.InvoiceTotal = totalAmount;
                existing.PaidAmount = paidAmount;
                existing.AmountOverdue = totalAmount - paidAmount;
                ApplyPaymentMethod(existing, paymentMethod);
                result.Updated++;
            }
            else
            {
                var c4 = new CRD4
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = first.DocDate,
                    DueDate = first.DocDueDate,
                    InvoiceTotal = totalAmount,
                    PaidAmount = paidAmount,
                    AmountOverdue = totalAmount - paidAmount,
                    BPId = bpId
                };
                ApplyPaymentMethod(c4, paymentMethod);
                toAdd.Add(c4);
                result.Inserted++;
            }
        }

        result.Skipped = sapItems.Count - result.Inserted - result.Updated;

        if (toAdd.Count > 0)
            await _db.CRD4.AddRangeAsync(toAdd, ct);

        await _db.SaveChangesAsync(ct);

        // 5. Tiến checkpoint = max(UpdateDate) để vòng sau không re-process.
        result.NewCheckpoint = sapItems.Max(i => i.UpdateDate).ToUniversalTime();
        await UpdateCheckpointAsync(checkpoint, result.NewCheckpoint, result, ct);

        _logger.LogInformation(
            "Delta sync done: {Total} from SAP, {Inserted} inserted, {Updated} updated, {Skipped} skipped, new checkpoint {Checkpoint:o}",
            result.TotalFromSap, result.Inserted, result.Updated, result.Skipped, result.NewCheckpoint);

        return result;
    }

    public async Task<SyncResult> SyncCardBalanceCRD4DeltaAsync(CancellationToken ct)
    {
        var checkpoint = await GetCheckpointAsync(JobKeyBalance, ct);
        var sinceUtc = (checkpoint?.LastSyncedAt ?? DateTime.UtcNow.AddDays(-30)) - SafetyOverlap;
        var since = sinceUtc.ToString("yyyy-MM-ddTHH:mm:ss");

        _logger.LogInformation("Delta sync card balance CRD4 since {Since}", since);

        // 1. BPs có UpdateDate >= since AND CurrentAccountBalance < 0 (có nợ).
        var filter = $"UpdateDate ge '{since}' and CardType eq 'cCustomer' and CurrentAccountBalance lt 0";

        var sapItems = await _sl.Request("BusinessPartners")
            .Filter(filter)
            .Select("CardCode,CurrentAccountBalance,UpdateDate")
            .OrderBy("UpdateDate")
            .WithPageSize(SapPageSize)
            .GetAllAsync<BPBalanceItem>();

        var result = new SyncResult { TotalFromSap = sapItems.Count };

        if (sapItems.Count == 0)
        {
            await UpdateCheckpointAsync(JobKeyBalance, checkpoint, DateTime.UtcNow, result, ct);
            return result;
        }

        // 2. Map CardCode → BPId trong 1 query.
        var cardCodes = sapItems.Select(i => i.CardCode).Distinct().ToArray();
        var bpMap = await _db.BP
            .Where(b => cardCodes.Contains(b.CardCode))
            .Select(b => new { b.Id, b.CardCode })
            .ToDictionaryAsync(x => x.CardCode!, x => x.Id, ct);

        // 3. Lấy CRD3 (payment method preference) cho các BP — 1 query.
        var bpIds = bpMap.Values.ToArray();
        var crd3Map = await _db.CRD3
            .Where(c => bpIds.Contains(c.BPId))
            .GroupBy(c => c.BPId)
            .Select(g => g.OrderByDescending(x => x.Balance).First())
            .ToDictionaryAsync(c => c.BPId, ct);

        // 4. Lấy CRD4 hiện có theo (BPId, InvoiceNumber == CardCode).
        var existingMap = await _db.CRD4
            .Where(c => bpIds.Contains(c.BPId) && cardCodes.Contains(c.InvoiceNumber))
            .ToDictionaryAsync(c => $"{c.BPId}:{c.InvoiceNumber}", ct);

        var toAdd = new List<CRD4>();
        foreach (var item in sapItems)
        {
            ct.ThrowIfCancellationRequested();
            if (!bpMap.TryGetValue(item.CardCode, out var bpId)) continue;
            crd3Map.TryGetValue(bpId, out var crd3);

            var key = $"{bpId}:{item.CardCode}";
            if (existingMap.TryGetValue(key, out var existing))
            {
                existing.InvoiceTotal = 0;
                existing.PaidAmount = -item.CurrentAccountBalance;
                existing.AmountOverdue = item.CurrentAccountBalance;
                if (crd3 != null)
                {
                    existing.PaymentMethodID = crd3.PaymentMethodID;
                    existing.PaymentMethodCode = crd3.PaymentMethodCode;
                    existing.PaymentMethodName = crd3.PaymentMethodName;
                }
                result.Updated++;
            }
            else
            {
                var c4 = new CRD4
                {
                    BPId = bpId,
                    InvoiceNumber = item.CardCode,
                    InvoiceTotal = 0,
                    PaidAmount = -item.CurrentAccountBalance,
                    AmountOverdue = item.CurrentAccountBalance,
                    PaymentMethodID = crd3?.PaymentMethodID,
                    PaymentMethodCode = crd3?.PaymentMethodCode,
                    PaymentMethodName = crd3?.PaymentMethodName
                };
                toAdd.Add(c4);
                result.Inserted++;
            }
        }

        if (toAdd.Count > 0)
            await _db.CRD4.AddRangeAsync(toAdd, ct);

        await _db.SaveChangesAsync(ct);

        result.NewCheckpoint = sapItems.Max(i => i.UpdateDate).ToUniversalTime();
        await UpdateCheckpointAsync(JobKeyBalance, checkpoint, result.NewCheckpoint, result, ct);

        _logger.LogInformation(
            "Delta sync card balance done: {Total} from SAP, {Inserted} inserted, {Updated} updated",
            result.TotalFromSap, result.Inserted, result.Updated);

        return result;
    }

    private static void ApplyPaymentMethod(CRD4 c4, string? code) => (c4.PaymentMethodID, c4.PaymentMethodCode, c4.PaymentMethodName) = code switch
    {
        "2" => (2, "PayCredit",    "Công nợ - Tín chấp"),
        "3" => (3, "PayGuarantee", "Công nợ - Bảo lãnh"),
        _   => (c4.PaymentMethodID, c4.PaymentMethodCode, c4.PaymentMethodName)
    };

    private Task<SyncCheckpoint?> GetCheckpointAsync(string jobKey, CancellationToken ct)
        => _db.SyncCheckpoints.FirstOrDefaultAsync(c => c.JobName == jobKey, ct);

    private Task<SyncCheckpoint?> GetCheckpointAsync(CancellationToken ct)
        => GetCheckpointAsync(JobKeyInvoice, ct);

    private Task UpdateCheckpointAsync(SyncCheckpoint? existing, DateTime newCheckpoint, SyncResult result, CancellationToken ct)
        => UpdateCheckpointAsync(JobKeyInvoice, existing, newCheckpoint, result, ct);

    private async Task UpdateCheckpointAsync(string jobKey, SyncCheckpoint? existing, DateTime newCheckpoint, SyncResult result, CancellationToken ct)
    {
        if (existing == null)
        {
            _db.SyncCheckpoints.Add(new SyncCheckpoint
            {
                JobName = jobKey,
                LastSyncedAt = newCheckpoint,
                LastRecordsProcessed = result.TotalFromSap,
                LastStatus = "Success",
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.LastSyncedAt = newCheckpoint;
            existing.LastRecordsProcessed = result.TotalFromSap;
            existing.LastStatus = "Success";
            existing.LastError = null;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
    }
}
