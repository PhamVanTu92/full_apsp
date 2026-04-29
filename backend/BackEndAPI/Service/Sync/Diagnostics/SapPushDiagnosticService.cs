using B1SLayer;
using BackEndAPI.Data;
using BackEndAPI.Models.Document;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Sync.Diagnostics;

public class SapPushDiagnosticService : ISapPushDiagnosticService
{
    private readonly AppDbContext _db;
    private readonly SLConnection _sl;
    private readonly ILogger<SapPushDiagnosticService> _logger;

    public SapPushDiagnosticService(AppDbContext db, SLConnection sl, ILogger<SapPushDiagnosticService> logger)
    {
        _db = db;
        _sl = sl;
        _logger = logger;
    }

    public async Task<SapPushDiagnostic> PushOneAsync(int docId, CancellationToken ct)
    {
        var diag = new SapPushDiagnostic { DocId = docId };

        var doc = await _db.ODOC
            .Include(x => x.ItemDetail)
            .FirstOrDefaultAsync(x => x.Id == docId, ct);

        if (doc == null)
        {
            diag.Conclusion = "Doc không tồn tại trong DB";
            return diag;
        }

        diag.InvoiceCode = doc.InvoiceCode;
        diag.CardCode = doc.CardCode;
        diag.IsSyncBefore = doc.IsSync;

        // 1. Test SAP login
        try
        {
            await _sl.LoginAsync();
            diag.SapLoginOk = true;
        }
        catch (Exception ex)
        {
            diag.SapLoginError = $"{ex.GetType().Name}: {ex.Message}";
            diag.Conclusion = "Không kết nối được SAP — kiểm tra network / credential / TLS cert";
            return diag;
        }

        // 2. Check xem doc đã tồn tại trên SAP (Drafts) chưa
        try
        {
            var draftQuery = await _sl.Request("Drafts")
                .Filter($"U_MDHPT eq '{doc.InvoiceCode?.Replace("'", "''")}'")
                .Select("DocEntry,U_MDHPT,DocStatus")
                .GetAsync<DraftCheckResponse>();

            diag.DraftCheckOk = true;
            diag.DraftCheckResponse = System.Text.Json.JsonSerializer.Serialize(draftQuery);

            if (draftQuery?.Value?.Count > 0)
            {
                diag.DocumentExistsAtSap = true;
                diag.SapDraftDocEntry = draftQuery.Value[0].DocEntry;
                diag.Conclusion = $"Doc đã tồn tại ở SAP (DocEntry={diag.SapDraftDocEntry}) " +
                                  $"nhưng IsSync local vẫn false → có thể fix bằng UPDATE ODOC SET IsSync=1, SapDocEntry={diag.SapDraftDocEntry} WHERE Id={docId}";
                return diag;
            }
        }
        catch (Exception ex)
        {
            diag.DraftCheckError = $"{ex.GetType().Name}: {ex.Message}";
        }

        // 3. Build payload preview (không thực sự push — chỉ diagnose)
        diag.PushPayloadSummary = $"CardCode={doc.CardCode}, InvoiceCode={doc.InvoiceCode}, " +
                                  $"Total={doc.Total}, Lines={doc.ItemDetail?.Count ?? 0}";

        diag.Conclusion = doc.ItemDetail == null || doc.ItemDetail.Count == 0
            ? "Doc không có ItemDetail — không thể push (data integrity issue)"
            : "Doc có thể push được nhưng đang chỉ diagnose — không thực hiện push thật. " +
              "Để push thật, dùng job SyncJob hoặc method legacy SyncToSapAsync.";

        return diag;
    }

    private class DraftCheckResponse
    {
        public List<DraftEntry>? Value { get; set; }
    }

    private class DraftEntry
    {
        public int DocEntry { get; set; }
        public string? U_MDHPT { get; set; }
        public string? DocStatus { get; set; }
    }
}
