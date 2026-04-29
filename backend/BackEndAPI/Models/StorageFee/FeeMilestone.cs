using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.StorageFee
{
    public class FeeMilestone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public int FromDay { get; set; }
        public int ToDay { get; set; }
        [NotMapped]
        [JsonIgnore]
        public List<StorageFeeLine>? StorageFeeLine {get; set;}
        [NotMapped]
        public bool isSuccess {get; set;} = false;
        [NotMapped]
        public string ErrorMessage {get; set;} = "";
    }
    [NotMapped]
    public class FeeMilestoneDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public int NumberOfDay { get; set; }
        public int FromDay { get; set; }
        public int ToDay { get; set; }
    }
}
