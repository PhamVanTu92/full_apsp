using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Data;
using BackEndAPI.Models.Other;

namespace BackEndAPI.Models.StorageFee
{
    public class StorageFee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UgpId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }


        [NotMapped]
        public List<StorageFeeLine> Lines { get; set; } = new List<StorageFeeLine>();

        [NotMapped]
        public string UgpName { get; set; } = "";

        [NotMapped]
        public string UgpDes { get; set; } = "";

    }

    [NotMapped]
    public class StorageFeeDto
    {
        public int Id { get; set; }
        public int UgpId { get; set; }
        public List<StorageFeeLine> Lines { get; set; } = new List<StorageFeeLine>();
        public string UgpName { get; set; } = "";
        public string UgpDes { get; set; } = "";
    }

    #region//----------------------------------
    public class WarehousePriceLinkFeeMilestone
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int FeeId { get; set; }
        [Required]
        public int StorageFeeId { get; set; }
        [Required]
        public double Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        [JsonIgnore]
        public StorageFee? StorageFeess { get; set; }
    }

    public class WarehousePriceLinkFeeMilestoneDto
    {
        public int Id { get; set; }
        public int FeeId { get; set; }
        public int StorageFeeId { get; set; }
        public double Price { get; set; }
        public StorageFee? StorageFees { get; set; }
    }

    

    #endregion
}
