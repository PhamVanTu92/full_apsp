using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndAPI.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAPI.Models.Approval_V2;

public enum ApprovalStatus
{
    Pending   = 0,
    Approved  = 1,
    Rejected  = 2,
    Cancelled = 3,
}

public enum ApprovalAction
{
    /// <summary>
    /// Chờ phê duyệt
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Phê duyệt
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Từ chối
    /// </summary>
    Rejected = 2
}

public class ApprovalWorkFlowConfiguration : IEntityTypeConfiguration<ApprovalWorkFlow>,
    IEntityTypeConfiguration<ApprovalWorkFlowLine>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkFlow> builder)
    {
        builder.HasOne(x => x.ApprovalLevel)
            .WithMany()
            .HasForeignKey(x => x.ApprovalLevelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ApprovalSample)
            .WithMany()
            .HasForeignKey(x => x.ApprovalSampleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.ApprovalWorkFlowLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ApprovalWorkFlowDocumentLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public void Configure(EntityTypeBuilder<ApprovalWorkFlowLine> builder)
    {
        builder.HasOne(x => x.ApprovalUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovalUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ApprovalLevel)
            .WithMany()
            .HasForeignKey(x => x.ApprovalLevelId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ApprovalWorkFlow
{
    public int Id { get; set; }
    public int DocId { get; set; }

    public int ApprovalSampleId { get; set; }
    public string? Description { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }

    public int ApprovalNumber { get; set; }
    public int DeclineNumber { get; set; }
    public int ApprovalLevelId { get; set; }
    public ApprovalLevel? ApprovalLevel { get; set; }
    public ApprovalSample? ApprovalSample { get; set; }

    public List<ApprovalWorkFlowDocumentLine> ApprovalWorkFlowDocumentLines { get; set; } = [];
    public List<ApprovalWorkFlowLine> ApprovalWorkFlowLines { get; set; } = [];

    public int? CreatorId { get; set; }
    public AppUser? Creator { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}

public class ApprovalWorkFlowDocumentLine
{
    public int Id { get; set; }
    public int FatherId { get; set; }
    public int DocId { get; set; }

    [NotMapped] public object? DocObj { get; set; }
    public DocumentEnum DocumentType { get; set; }

    [MaxLength(255)] public string? DocCode { get; set; } = string.Empty;
}

public class ApprovalWorkFlowLine
{
    public int Id { get; set; }

    public int FatherId { get; set; }

    public int ApprovalUserId { get; set; }

    public int ApprovalLevelId { get; set; }

    /// <summary>
    /// P: Pending, A: Approved, R: Rejected
    /// </summary>
    public ApprovalAction Status { get; set; } = ApprovalAction.Pending;

    [MaxLength(255)] public string? Note { get; set; }

    public int SortId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public AppUser? ApprovalUser { get; set; }
    public ApprovalLevel? ApprovalLevel { get; set; }
}