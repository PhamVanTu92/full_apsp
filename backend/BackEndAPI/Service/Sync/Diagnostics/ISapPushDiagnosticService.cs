namespace BackEndAPI.Service.Sync.Diagnostics;

/// <summary>
/// Diagnostic tool — force-push 1 ODOC doc lên SAP với log chi tiết.
/// Mục đích: debug stuck-doc thay vì đoán mò.
/// Dùng qua admin endpoint POST /api/admin/sync/push/{docId}
/// </summary>
public interface ISapPushDiagnosticService
{
    Task<SapPushDiagnostic> PushOneAsync(int docId, CancellationToken ct);
}

public class SapPushDiagnostic
{
    public int DocId { get; set; }
    public string? InvoiceCode { get; set; }
    public string? CardCode { get; set; }
    public bool IsSyncBefore { get; set; }
    public bool IsSyncAfter { get; set; }

    public bool SapLoginOk { get; set; }
    public string? SapLoginError { get; set; }

    public bool DraftCheckOk { get; set; }
    public string? DraftCheckResponse { get; set; }
    public string? DraftCheckError { get; set; }

    public bool DocumentExistsAtSap { get; set; }
    public int? SapDraftDocEntry { get; set; }

    public string? PushPayloadSummary { get; set; }
    public bool PushOk { get; set; }
    public string? PushError { get; set; }
    public int? NewSapDocEntry { get; set; }

    public string? Conclusion { get; set; }
}
