using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.BusinessPartners;
using Microsoft.VisualBasic;

namespace BackEndAPI.Models;

public class DebtReconciliation
{
    [Key] public int Id { get; set; }

    [MaxLength(255), Required] public string Name { get; set; } = string.Empty;
    [MaxLength(255)] public string Code { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    [JsonIgnore] public BP? Customer { get; set; }

    [NotMapped] public string CustomerName => Customer?.CardName ?? "unknown";

    public int UserId { get; set; }

    [JsonIgnore] public AppUser? User { get; set; }
    public string? CreatorName => User?.FullName;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(5)] public string Status { get; set; } = "P";

    public List<DebtReconciliationAttachment>? Attachments { get; set; } = new List<DebtReconciliationAttachment>();
    public List<DebtReconciliationAttachment>? BpAttachments { get; set; } = [];

    public DateTime? ConfirmationDate { get; set; } 
    public DateTime? SendingDate { get; set; } 

    public string? Reason { get; set; } = string.Empty;

    public string? Note { get; set; } = string.Empty;
    
    public string? RejectReason { get; set; }
    // public string? ClientNote { get; set; } = string.Empty;
}

public class DebtReconciliationAttachment
{
    [Key] public int Id { get; set; }
    public int DebtId { get; set; }
    [MaxLength(255)] public string FileName { get; set; } = string.Empty;

    [MaxLength(255)] public string FileGuid { get; set; } = string.Empty;
    [MaxLength(255)] public string FilePath { get; set; } = string.Empty;
    [MaxLength(255)] public string FileUrl { get; set; } = string.Empty;
    [MaxLength(15)]
    public string Type { get; set; } = string.Empty;
}
