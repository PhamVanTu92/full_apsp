using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Document;

namespace BackEndAPI.Models.Approval;

public class Approval
{
    public int Id { get; init; }
    public int WtmId { get; set; }
    public int DocId { get; init; }
    public int? TransType { get; init; }
    public int CurStep { get; set; }
    public int MaxReqr { get; set; } = 1; // Default to 1
    public int MaxRejReqr { get; set; } = 1; // Default to 1
    public string Status { get; set; } = "P"; // Default to "P"
    [NotMapped] public List<ApprovalLine> Lines { get; set; } = new List<ApprovalLine>();

    public int? ActorId { get; init; }

    // [NotMapped]
    // public object Obj { get; set; } // Use 'object' type as a generic placeholder
    [NotMapped] public ODOC? Document { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [NotMapped] public OWTM? Owtm { get; init; }
    [JsonIgnore] public AppUser? Actor { get; init; }
    [NotMapped]
    public bool IsApp { get; set; }
}


public class ApprovalLine
{
    public int Id { get; init; }
    public int WddId { get; init; }
    public int StepCode { get; init; }
    public int UserId { get; init; }
    [JsonIgnore] [NotMapped] public OWST? Owst { get; init; }

    public int WstId { get; set; }
    [NotMapped] public string StageName => Owst?.Name;
    public string Status { get; set; } = "P";
    public string Note { get; set; } = "";
    public DateTime? ApprovalAt { get; set; }
    [NotMapped] public Approval? Approval { get; init; }
    public Models.Account.AppUser? User { get; set; }
}