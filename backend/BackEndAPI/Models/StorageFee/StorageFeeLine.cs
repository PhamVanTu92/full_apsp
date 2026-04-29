using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.StorageFee
{
    public class StorageFeeLine
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
        public StorageFee? StorageFee { get; set; }
        [NotMapped]
        public FeeMilestone? FeeMilestone { get; set; }
    }
    [NotMapped]
    public class StorageFeeLineDto
    {
        public int Id { get; set; }
        public int FeeId { get; set; }
        public int StorageFeeId { get; set; }
        public double Price { get; set; }
        [JsonIgnore]
        public StorageFeeDto? StorageFee { get; set; }

        [NotMapped]
        [JsonIgnore]
        public FeeMilestone? FeeMilestone { get; set; }
    }
}
