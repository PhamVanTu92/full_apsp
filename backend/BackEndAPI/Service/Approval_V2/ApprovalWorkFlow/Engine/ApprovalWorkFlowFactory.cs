using System;
using System.Collections.Generic;
using System.Linq;
using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Service.Approval_V2.ApprovalWorkFlow.Engine;

public interface IApprovalWorkFlowFactory
{
    IApprovalWorkFlowEngine GetEngine(DocumentEnum documentType);
}

public class ApprovalWorkFlowFactory : IApprovalWorkFlowFactory
{
    private readonly IEnumerable<IApprovalWorkFlowEngine> _engines;

    public ApprovalWorkFlowFactory(IEnumerable<IApprovalWorkFlowEngine> engines)
    {
        _engines = engines;
    }

    public IApprovalWorkFlowEngine GetEngine(DocumentEnum documentType)
    {
        var engine = _engines.FirstOrDefault(e => e.DocumentType == documentType);
        if (engine is null) throw new NotSupportedException($" Không hỗ trợ loại chứng từ {documentType}");
        return engine;
    }
}