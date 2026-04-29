using System.Security.Claims;
using BackEndAPI.Data;
using BackEndAPI.Models.Approval_V2;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;

public class RequestPickUpItemsWorkFlow(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    : BaseWorkFlowEngineService


{
    public override DocumentEnum DocumentType => DocumentEnum.RequestPickUpItems;

    public override async Task<List<Models.Approval_V2.ApprovalSample>> IsApprovalWorkFlow(int docId)
    {
        var user   = httpContextAccessor.HttpContext?.User;
        var userId = int.Parse(user!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var requestPickup = await context.ODOC.FirstOrDefaultAsync(x => x.Id == docId && x.ObjType == 1250000001);
        if (requestPickup is null) return [];

        var isExist = await context.ApprovalSamples
            .Include(x => x.ApprovalSampleMembersLines)
            .ThenInclude(x => x.Creator)
            .Include(x => x.ApprovalSampleDocumentsLines)
            .Include(x => x.ApprovalSampleProcessesLines)
            .ThenInclude(x => x.ApprovalLevel)
            .ThenInclude(x => x!.ApprovalLevelLines)
            .Where(x => x.IsActive
                // && x.ApprovalSampleMembersLines.Select(line => line.CreatorId).Contains(userId)
                && ((x.IsOverdueDebt && requestPickup.LimitOverDue == true) ||
                    (x.IsDebtLimit   && requestPickup.LimitRequire == true))
                && x.ApprovalSampleDocumentsLines.Any(documentsLine =>
                    documentsLine.Document == DocumentEnum.RequestPickUpItems)
            )
            .ToListAsync();
        return isExist;
    }

    public override async Task<bool> HandleAfterApproveAsync(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow)
    {
        var purchaseOrderId = approvalWorkFlow.ApprovalWorkFlowDocumentLines.First().DocId;
        var purchaseOrder   = await context.ODOC.FirstOrDefaultAsync(x => x.Id == purchaseOrderId);
        if (purchaseOrder is null) return false;

        purchaseOrder.Status         = "CXN";
        purchaseOrder.ApprovalStatus = "A";
        await context.SaveChangesAsync();
        return true;
    }

    public override Task<bool> HandleAftherDeclineAsync(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow)
    {
        return Task.FromResult(true);
    }

    public override async Task<object?> GetEntityAsync(int docId)
    {
        var result = await context.ODOC.FirstOrDefaultAsync(x => x.Id == docId);
        return result;
    }

    public override async Task<string> GetDocStatus(int docId)
    {
        var result = await context.ODOC.FirstOrDefaultAsync(x => x.Id == docId);
        if (result is null) return "";
        return result.Status ?? "";
    }
}