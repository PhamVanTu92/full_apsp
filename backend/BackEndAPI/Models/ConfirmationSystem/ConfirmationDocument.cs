using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.ConfirmationSystem
{
    public enum DocumentStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class ConfirmationDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CardId { get; set; }
        [Required]
        [StringLength(50)]
        public string CardCode { get; set; }
        [Required]
        [StringLength(255)]
        public string CardName { get; set; }

        [Required]
        public string FileUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? SentDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string? Note { get; set; }

        public virtual ICollection<ConfirmationLog> Logs { get; set; } = new List<ConfirmationLog>();
    }

    public class ConfirmationLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }
            
        [ForeignKey("DocumentId")]
        public virtual ConfirmationDocument Document { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } // Upload, Send, Approve, Reject

        [Required]
        [StringLength(100)]
        public string ActionBy { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }
    }
    public class ConfirmationDocumentNew
    {
        [Required]
        public IFormFile File { get; set; }
        [Required]
        public int CardId { get; set; }
        [Required]
        [StringLength(50)]
        public string CardCode { get; set; }
        [Required]
        [StringLength(255)]
        public string CardName { get; set; }

        public string? Note { get; set; }
    }
    public class ActionRequest
    {
        public int Id { get; set; }
        public string? Note { get; set; }
    }
}
