using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Service;

public interface IApprovalWorkFlowService
{
    Task<(List<Models.Approval_V2.ApprovalWorkFlow>, int)> GetAllAsync(GridifyQuery gridifyQuery, string? search);
    Task<(Models.Approval_V2.ApprovalWorkFlow?, Mess?)>    GetByIdAsync(int id);

    Task<List<Models.Approval_V2.ApprovalWorkFlow>> CreateAsync(List<IdAndTypeDocDto> documentId, int userId,
        List<Models.Approval_V2.ApprovalSample> approvalWorkFlows);

    Task<(bool, Mess?)> ApprovalAsync(CreateApprovalRequest request);

    Task<List<Models.Approval_V2.ApprovalSample>> CheckApprovalAsync(int documentId, DocumentEnum docType);
}