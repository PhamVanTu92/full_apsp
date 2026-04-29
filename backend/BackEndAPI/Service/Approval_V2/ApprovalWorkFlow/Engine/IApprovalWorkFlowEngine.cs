using System.Collections.Generic;
using System.Threading.Tasks;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;

public interface IApprovalWorkFlowEngine
{
    DocumentEnum DocumentType { get; }
    Task<List<Models.Approval_V2.ApprovalSample>> IsApprovalWorkFlow(int docId);

    Task<bool> HandleAfterApproveAsync(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow);
    Task<bool> HandleAftherDeclineAsync(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow);

    Task<List<Models.Approval_V2.ApprovalWorkFlow>> CreateApprovalWorkFlow(List<IdAndTypeDocDto> documentIds,
        int userId,
        List<Models.Approval_V2.ApprovalSample> approvalSamples);

    Task<object?> GetEntityAsync(int docId);

    Task<string> GetDocStatus(int docId);
}