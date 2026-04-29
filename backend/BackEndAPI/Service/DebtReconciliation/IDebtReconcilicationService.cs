using Gridify;

namespace BackEndAPI.Service.DebtReconciliation;

public interface IDebtReconcilicationService
{
    Task<(List<Models.DebtReconciliation>, int)> GetDebtReconciliations(string? search,
        GridifyQuery gridifyQuery, int bpId = 0);

    Task<Models.DebtReconciliation> GetDebtReconciliationById(int id);
    Task<Models.DebtReconciliation> CreateDebtReconciliation(Models.DebtReconciliation debtReconciliation);
    Task<Models.DebtReconciliation> AddAttachmentToDebtReconciliation(int id, List<IFormFile> files, string type = "system");
    Task<Models.DebtReconciliation> RemoveAttachmentToDebtReconciliation(int id, List<int> ids);
    Task<Models.DebtReconciliation> ChangeStatusDebtReconiliation(int id, string status, string? rejectReason = null);
    Task<Models.DebtReconciliation> UpdateDebtReconiliation(int id, Models.DebtReconciliation debt);
}