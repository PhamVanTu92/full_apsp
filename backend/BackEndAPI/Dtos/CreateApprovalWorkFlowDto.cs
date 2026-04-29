using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Dtos;

public class CreateApprovalWorkFlowDto
{
    public int DocId { get; set; }
    public DocumentEnum DocType { get; set; }
}