using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Dtos;

public class CreateApprovalSample
{
    /// <summary>
    /// Tên mẫu phê duyệt
    /// </summary>
    public string ApprovalSampleName { get; set; } = null!;

    /// <summary>
    /// Mô tả phê duyệt
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Hạn mức công nợ?
    /// </summary>
    public bool IsDebtLimit { get; set; }

    /// <summary>
    /// Nợ quá hạn
    /// </summary>
    public bool IsOverdueDebt { get; set; }

    /// <summary>
    /// Khác
    /// </summary>
    public bool IsOther { get; set; }

    public List<int> ApprovalSampleMembersLines { get; set; } = [];

    public List<int> ApprovalSampleProcessesLines { get; set; } = [];

    public List<DocumentEnum> ApprovalSampleDocumentsLines { get; set; } = [];

    public List<int> ApprovalSampleOcrgLines { get; set; } = [];
}

public class CreateApprovalSampleDocumentsLines
{
}