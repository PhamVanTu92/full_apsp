using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEndAPI.Dtos;
using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;

public abstract class BaseWorkFlowEngineService : IApprovalWorkFlowEngine
{
    public abstract DocumentEnum DocumentType { get; }
    public abstract Task<List<Models.Approval_V2.ApprovalSample>> IsApprovalWorkFlow(int docId);
    public abstract Task<bool> HandleAfterApproveAsync(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow);
    public abstract Task<bool> HandleAftherDeclineAsync(Models.Approval_V2.ApprovalWorkFlow approvalWorkFlow);

    public abstract Task<object?> GetEntityAsync(int docId);
    public abstract Task<string>  GetDocStatus(int docId);

    public Task<List<Models.Approval_V2.ApprovalWorkFlow>> CreateApprovalWorkFlow(List<IdAndTypeDocDto> documentIds,
        int userId, List<Models.Approval_V2.ApprovalSample> approvalSamples)
    {
        var approvalWorkFlows = new List<Models.Approval_V2.ApprovalWorkFlow>();

        foreach (var approvalSample in approvalSamples)
        {
            var approvalLevel = approvalSample.ApprovalSampleProcessesLines.FirstOrDefault();
            if (approvalLevel is null) throw new Exception("Không tìm thấy người duyệt");

            var item = new Models.Approval_V2.ApprovalWorkFlow
            {
                DocId            = 1,
                ApprovalSampleId = approvalSample.Id,
                Description      = "",
                ApprovalStatus   = ApprovalStatus.Pending,
                ApprovalNumber   = approvalLevel.ApprovalLevel!.ApprovalNumber,
                DeclineNumber    = approvalLevel.ApprovalLevel.DeclineNumber,
                ApprovalLevelId  = approvalLevel.ApprovalLevelId,
                CreatorId        = userId,
                ApprovalWorkFlowDocumentLines = documentIds.Select(x =>
                    new ApprovalWorkFlowDocumentLine
                    {
                        DocId        = x.Id,
                        DocumentType = x.Type,
                        DocCode      = x.DocType
                    }).ToList(),
                ApprovalWorkFlowLines = approvalLevel.ApprovalLevel.ApprovalLevelLines.Select(x =>
                    new ApprovalWorkFlowLine
                    {
                        ApprovalUserId  = x.ApprovalUserId,
                        ApprovalLevelId = approvalLevel.ApprovalLevelId,
                        Status          = ApprovalAction.Pending,
                        SortId          = 1,
                    }).ToList()
            };

            approvalWorkFlows.Add(item);
        }
 return Task.FromResult(approvalWorkFlows); } }