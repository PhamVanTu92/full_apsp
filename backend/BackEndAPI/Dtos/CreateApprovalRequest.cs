using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Dtos;

public class CreateApprovalRequest
{
    public int ApprovalDecisionId { get; set; }
    public int ApprovalDecisionLineId { get; set; }
    public ApprovalAction Status { get; set; }
    public string? Note { get; set; }
}