using BackEndAPI.Data;
using BackEndAPI.Service.Sync.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Controllers;

/// <summary>
/// Admin tools để debug + xử lý sync — KHÔNG dành cho user thường.
/// TODO: thêm role check riêng cho admin (hiện chỉ Authorize chung).
/// </summary>
[ApiController]
[Route("api/admin/sync")]
[Authorize]
[EnableRateLimiting("admin-sync")]
public class AdminSyncController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISapPushDiagnosticService _diagnostic;

    public AdminSyncController(AppDbContext db, ISapPushDiagnosticService diagnostic)
    {
        _db = db;
        _diagnostic = diagnostic;
    }

    /// <summary>
    /// Liệt kê các doc đang stuck (chưa sync) — hỗ trợ filter theo ngày.
    /// </summary>
    [HttpGet("stuck")]
    public async Task<IActionResult> GetStuckDocs(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var query = _db.ODOC
            .AsNoTracking()
            .Where(x => x.Status == "DXN" && x.ObjType == 22 && x.IsSync == false);

        if (from.HasValue) query = query.Where(x => x.DocDate >= from);
        if (to.HasValue) query = query.Where(x => x.DocDate <= to);

        var totalCount = await query.CountAsync(ct);
        var docs = await query
            .OrderByDescending(x => x.DocDate)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.InvoiceCode,
                x.CardCode,
                x.CardName,
                x.Status,
                x.IsSync,
                x.IsCheck,
                x.SapDocEntry,
                x.DocDate,
                x.Total
            })
            .ToListAsync(ct);

        return Ok(new { totalCount, returned = docs.Count, docs });
    }

    /// <summary>
    /// Diagnostic: kiểm tra 1 doc, login SAP, check tồn tại trên SAP,
    /// trả về kết luận tại sao stuck. KHÔNG push thật.
    /// </summary>
    [HttpGet("diagnose/{docId:int}")]
    public async Task<IActionResult> Diagnose(int docId, CancellationToken ct)
    {
        var result = await _diagnostic.PushOneAsync(docId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Bulk fix — đánh dấu các doc đã tồn tại ở SAP (theo SapDocEntry user cung cấp)
    /// là đã sync. Dùng khi diagnose trả về "doc đã tồn tại ở SAP nhưng local chưa sync".
    /// </summary>
    [HttpPost("mark-synced")]
    public async Task<IActionResult> MarkSynced(
        [FromBody] MarkSyncedRequest request,
        CancellationToken ct)
    {
        if (request?.DocIds == null || request.DocIds.Length == 0)
            return BadRequest(new { error = "DocIds required" });

        var docs = await _db.ODOC.Where(x => request.DocIds.Contains(x.Id)).ToListAsync(ct);
        foreach (var d in docs)
        {
            d.IsSync = true;
            if (request.SapDocEntryByLocalId != null &&
                request.SapDocEntryByLocalId.TryGetValue(d.Id, out var entry))
            {
                d.SapDocEntry = entry;
            }
        }
        await _db.SaveChangesAsync(ct);

        return Ok(new { updated = docs.Count, ids = docs.Select(d => d.Id) });
    }

    public class MarkSyncedRequest
    {
        public int[] DocIds { get; set; } = Array.Empty<int>();
        public Dictionary<int, int>? SapDocEntryByLocalId { get; set; }
    }
}
