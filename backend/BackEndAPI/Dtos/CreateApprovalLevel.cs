namespace BackEndAPI.Dtos;

public class CreateApprovalLevel
{
    public string ApprovalLevelName { get; set; } = null!;
    public string? Description { get; set; }

    public int ApprovalNumber { get; set; }
    public int DeclineNumber { get; set; }
    public bool IsActive { get; set; }
    public List<int> ApprovalLevelLines { get; set; } = [];
}