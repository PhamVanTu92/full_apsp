using BackEndAPI.Models.BPGroups;
using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.BusinessPartners
{
    public class BPDTO
    {
        public int Id { get; set; }
        public int? Series { get; set; }
        [MaxLength(50)]
        public string? CardCode { get; set; }
        [MaxLength(254)]
        public string? CardName { get; set; }
        [MaxLength(254)]
        public string? FrgnName { get; set; }
        [MaxLength(50)]
        public string? CardType { get; set; }
        [MaxLength(50)]
        public string? Type { get; set; }
        public int? GroupCode { get; set; } = -1;
        public string GroupName { get; set; }
        [MaxLength(25)]
        public string? LicTradNum { get; set; }
        public bool IsBusinessHouse { get; set; }
        public bool IsInterCom { get; set; }
        public string? Avatar { get; set; }
        [MaxLength(50)]
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(254)]
        public string? Address { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(254)]
        public string? LocationName { get; set; }
        public int? AreaId { get; set; }
        [MaxLength(254)]
        public string? AreaName { get; set; }
        [MaxLength(50)]
        public string? Email { get; set; }
        [MaxLength(50)]
        public string? Phone { get; set; }
        [MaxLength(100)]
        public string? Person { get; set; }
        public string? Note { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
        public ICollection<CRD1> CRD1 { get; set; }
    }
}
