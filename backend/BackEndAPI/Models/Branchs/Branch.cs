using BackEndAPI.Models.Other;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Branchs
{
    public class Branch
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string BranchName { get; set; }
        [MaxLength(25)]
        public string? Phone { get; set; }
        [MaxLength(25)]
        public string? Phone1 { get; set; }
        public int? TimeZoneId { get; set; }
        [MaxLength(254)]
        public string? TimeZone { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(254)]
        public string? LocationName { get; set; }
        public int? AreaId { get; set; }
        [MaxLength(254)]
        public string? AreaName { get; set; }
        [MaxLength(1000)]
        public string? Address {  get; set; }
        [MaxLength(25)]
        public string? Email { get; set; }
        [MaxLength(25)]
        public string Status { get; set; } = "A";
        public ICollection<BranchAddress>? BranchAddress { get; set; }
    }
    public static class BranchList
    {
        public static List<Branch> GetBranch()
        {
            List<Branch> listBranch = new List<Branch>
            {
                new Branch { Id = 1,BranchName = "Chi nhánh Trung tâm"}
            };
            return listBranch;
        }
    }
    public class BranchAddress
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int LocationId { get; set; }
        [MaxLength(254)]
        public string LocationName { get; set; }
        public int AreaId { get; set; }
        [MaxLength(254)]
        public string AreaName { get; set; }
        [MaxLength(25)]
        public string Phone { get; set; }
        [MaxLength(1000)]
        public string Address { get; set; }
        [NotMapped]
        public string? Status { get; set; }
        [JsonIgnore]
        public Branch? Branch { get; set; }

    }

}
