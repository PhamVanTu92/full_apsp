using BackEndAPI.Models.Account;
using BackEndAPI.Models.Unit;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Approval
{
    public class OWST
    {
        [Key] public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; }
        [MaxLength(254)] public string Remarks { get; set; } = "";
        public int MaxReqr { get; set; }
        public int MaxRejReqr { get; set; }

        public ICollection<WST1>? WST1 { get; set; }
        [JsonIgnore] [NotMapped] public List<ApprovalLine>? ApprovalLines { get; set; }
    }

    public class WST1
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int UserId { get; set; }
        public AppUser? User { get; set; }
        [NotMapped] [MaxLength(100)] public string? Status { get; set; }
        [JsonIgnore] public OWST? OWST { get; set; }
    }

    public class OWTM
    {
        public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; }

        [MaxLength(254)] public string Remarks { get; set; }
        public List<AppUser> RUsers { get; set; } = [];
        [NotMapped] public List<int> RUserIds { get; set; } = [];

        public bool Conds { get; set; }
        public bool Active { get; set; }
        public ICollection<WTM1>? WTM1 { get; set; }
        public ICollection<WTM2>? WTM2 { get; set; }
        public ICollection<WTM3>? WTM3 { get; set; }
        public WTM4? WTM4 { get; set; }
        public ICollection<Approval>? Approval { get; set; }
    }

    public class WTM1
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int UserId { get; set; }
        public AppUser? User { get; set; }
        [NotMapped] [MaxLength(100)] public string? Status { get; set; }
        [JsonIgnore] public OWTM? OWTM { get; set; }
    }

    public class WTM2
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int WtsId { get; set; }
        public int Step { get; set; }
        public OWST? OWST { get; set; }
        public int Sort { get; set; }
        [MaxLength(254)] public string? Remarks { get; set; }
        [NotMapped] [MaxLength(100)] public string? Status { get; set; }
        [JsonIgnore] public OWTM? OWTM { get; set; }
    }

    public class WTM3
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int TransType { get; set; }
        [NotMapped] [MaxLength(100)] public string? Status { get; set; }
        [JsonIgnore] public OWTM? OWTM { get; set; }
    }

    public class WTM4
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public bool Litmit { get; set; }
        public bool LitmitOver { get; set; }
        public bool Other { get; set; }
        [NotMapped] [MaxLength(100)] public string? Status { get; set; }
        [JsonIgnore] public OWTM? OWTM { get; set; }
    }
}