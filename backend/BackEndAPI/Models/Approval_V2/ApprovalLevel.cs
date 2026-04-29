using BackEndAPI.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackEndAPI.Models.Approval_V2;

public class ApprovalLevelConfiguration : IEntityTypeConfiguration<ApprovalLevel>,
    IEntityTypeConfiguration<ApprovalLevelLine>
{
    public void Configure(EntityTypeBuilder<ApprovalLevel> builder)
    {
        builder.HasMany(x => x.ApprovalLevelLines)
            .WithOne()
            .HasForeignKey(x => x.FatherId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ApprovalLevelLine> builder)
    {
        builder.HasOne(x => x.ApprovalUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovalUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

/// <summary>
/// Cấp phê duyệt v2
/// </summary>
public class ApprovalLevel
{
    public int Id { get; set; }
    public string ApprovalLevelName { get; set; } = null!;
    public string? Description { get; set; }

    public int ApprovalNumber { get; set; }
    public int DeclineNumber { get; set; }
    public bool IsActive { get; set; }
    public List<ApprovalLevelLine> ApprovalLevelLines { get; set; } = [];
}

public class ApprovalLevelLine
{
    public int Id { get; set; }
    public int FatherId { get; set; }

    public int ApprovalUserId { get; set; }

    public AppUser? ApprovalUser { get; set; }
}