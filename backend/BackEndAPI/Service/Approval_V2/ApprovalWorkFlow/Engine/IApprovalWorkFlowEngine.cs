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

    /// <summary>
    /// Batch variant of <see cref="GetEntityAsync"/> — returns a map from
    /// docId to entity for a set of ids. Used by callers that would
    /// otherwise produce N+1 queries (one round-trip per row of a paged
    /// list). Implementations should issue a single query with WHERE Id IN
    /// (...) and return only ids that were found.
    /// </summary>
    Task<Dictionary<int, object?>> GetEntitiesAsync(IReadOnlyCollection<int> docIds);

    Task<string> GetDocStatus(int docId);
}