using BackEndAPI.Service.Sap.Models;

namespace BackEndAPI.Service.Sap;

/// <summary>
/// Wrapper trên B1SLayer SLConnection — semantically meaningful methods cho SAP B1.
/// Service business (DocumentService, BPSyncService) gọi qua đây thay vì HttpWebRequest.
///
/// Status: POC, mới có 1 method. Xem docs/WEBREQUEST_MIGRATION.md cho plan đầy đủ.
/// </summary>
public interface ISapClient
{
    /// <summary>
    /// Query SAP Drafts theo InvoiceCode local (field U_MDHPT custom của APSP).
    /// </summary>
    Task<List<SapDraftSummary>> GetDraftsByInvoiceCodeAsync(string invoiceCode, CancellationToken ct);
}
