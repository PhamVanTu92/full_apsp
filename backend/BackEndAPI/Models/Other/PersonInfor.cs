using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Other
{
    public class PersonInfor
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string PersonName { get; set; }
        [MaxLength(10)]
        public string PersonType { get; set; }
        [MaxLength(20)]
        public string Phone { get; set; }
        [MaxLength(254)]
        public string? Address { get; set; }
        [MaxLength(254)]
        public string? Country { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(50)]
        public string? Ward { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
    }
    public class PersonInforAdd
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string PersonName { get; set; }
        [MaxLength(10)]
        public string PersonType { get; set; }
        [MaxLength(20)]
        public string Phone { get; set; }
        [MaxLength(254)]
        public string? Address { get; set; }
        [MaxLength(254)]
        public string? Country { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(50)]
        public string? Ward { get; set; }
        [MaxLength(50)]
        public string? Creator { get; set; }
    }
    public class PersonInforUpdate
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string PersonName { get; set; }
        [MaxLength(10)]
        public string PersonType { get; set; }
        [MaxLength(20)]
        public string Phone { get; set; }
        [MaxLength(254)]
        public string? Address { get; set; }
        [MaxLength(254)]
        public string? Country { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(50)]
        public string? Ward { get; set; }
        [MaxLength(50)]
        public string? Updator { get; set; }
    }
}
