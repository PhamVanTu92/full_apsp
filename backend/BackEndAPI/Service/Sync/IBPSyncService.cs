using BackEndAPI.Service.Sync.Models;

namespace BackEndAPI.Service.Sync;

public interface IBPSyncService
{
    /// <summary>
    /// Đồng bộ CRD4 (chi tiết công nợ theo invoice) từ SAP về local DB theo cơ chế delta:
    /// chỉ lấy <c>Invoices</c> có UpdateDate >= last checkpoint thay vì foreach toàn bộ BP.
    /// Dùng cho SyncBPCRD4Job (cron 30s).
    /// </summary>
    Task<SyncResult> SyncCRD4DeltaAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Đồng bộ CRD4 ở mức "current account balance" theo CardCode (1 row per BP, InvoiceNumber == CardCode).
    /// Chỉ lấy BP có UpdateDate >= last checkpoint AND CurrentAccountBalance &lt; 0.
    /// Dùng cho SyncDebJob (cron 2h).
    /// </summary>
    Task<SyncResult> SyncCardBalanceCRD4DeltaAsync(CancellationToken cancellationToken);
}
