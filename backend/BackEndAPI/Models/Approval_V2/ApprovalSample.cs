using BackEndAPI.Models.Account;
using BackEndAPI.Models.BPGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAPI.Models.Approval_V2;

public class ApprovalSampleConfiguration : IEntityTypeConfiguration<ApprovalSample>,
    IEntityTypeConfiguration<ApprovalSampleMembersLine>,
    IEntityTypeConfiguration<ApprovalSampleProcessesLine>,
    IEntityTypeConfiguration<ApprovalSampleOcrgLine>
{
    public void Configure(EntityTypeBuilder<ApprovalSample> builder)
    {
        builder.HasMany(x => x.ApprovalSampleDocumentsLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ApprovalSampleMembersLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ApprovalSampleProcessesLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ApprovalSampleOcrgLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ApprovalSampleMembersLine> builder)
    {
        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.NoAction);
    }


    public void Configure(EntityTypeBuilder<ApprovalSampleProcessesLine> builder)
    {
        builder.HasOne(x => x.ApprovalLevel)
            .WithMany()
            .HasForeignKey(x => x.ApprovalLevelId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public void Configure(EntityTypeBuilder<ApprovalSampleOcrgLine> builder)
    {
        builder.HasOne(x => x.BusinessPartnerGp)
            .WithMany()
            .HasForeignKey(x => x.BusinessPartnerGpId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public enum DocumentEnum
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    PurchaseOrder = 1,

    /// <summary>
    /// Yêu cầu lấy hàng gửi
    /// </summary>
    RequestPickUpItems = 2,

    /// <summary>
    /// Yêu cầu lấy mẫu thử nghiệm
    /// </summary>
    RequestForTestingSample = 3,
    /// <summary>
    /// Yêu cầu trả hàng
    /// </summary>
    RequestReturn = 4,
}

// <summary>
/// Mẫu phê duyệt
/// </summary>
public class ApprovalSample
{
    public int Id { get; set; }

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

    public List<ApprovalSampleMembersLine> ApprovalSampleMembersLines { get; set; } = [];
    public List<ApprovalSampleDocumentsLine> ApprovalSampleDocumentsLines { get; set; } = [];
    public List<ApprovalSampleProcessesLine> ApprovalSampleProcessesLines { get; set; } = [];
    public List<ApprovalSampleOcrgLine> ApprovalSampleOcrgLines { get; set; } = [];
}

public class ApprovalSampleMembersLine
{
    public int Id { get; set; }
    public int FatherId { get; set; }

    /// <summary>
    /// Id Người tạo
    /// </summary>
    public int CreatorId { get; set; }

    /// <summary>
    /// Ngưười tạo
    /// </summary>
    public AppUser? Creator { get; set; }
}

public class ApprovalSampleOcrgLine
{
    public int Id { get; set; }
    public int FatherId { get; set; }
    public int BusinessPartnerGpId { get; set; }
    public OCRG? BusinessPartnerGp { get; set; }
}

public class ApprovalSampleDocumentsLine
{
    public int Id { get; set; }
    public int FatherId { get; set; }

    /// <summary>
    /// Loại chứng từ
    /// </summary>
    public DocumentEnum? Document { get; set; }
}

public class ApprovalSampleProcessesLine
{
    public int Id { get; set; }
    public int FatherId { get; set; }

    /// <summary>
    /// Id Cấp phê duyệt
    /// </summary>
    public int ApprovalLevelId { get; set; }

    public ApprovalLevel? ApprovalLevel { get; set; }
}