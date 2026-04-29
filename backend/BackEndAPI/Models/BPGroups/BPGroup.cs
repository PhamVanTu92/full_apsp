using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.BusinessPartners;

namespace BackEndAPI.Models.BPGroups
{
    public class OCRG
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string GroupType { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OCRG? Parent { get; set; }
        public ICollection<OCRG>? Child { get; set; }
        public bool IsSelected { get; set; }

        public int SAPCode { get; set; } = 0;
      

        public List<BP> Customers { get; set; } = new List<BP>();
        public bool IsOneOfThem { get; set; } = false;

        public List<ConditionCusGroup> ConditionCusGroups { get; set; } = new List<ConditionCusGroup>();

    }

    public class ConditionCusGroup
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }
        // public string Operator { get; set; } = "";
        public bool IsEqual { get; set; }
        public string TypeCondition { get; set; } = "";
        public List<ConditionCusGroupValue> CondValues { get; set; } = new List<ConditionCusGroupValue>();
        [NotMapped]
        public List<string> Values { get; set; } = new List<string>();
    }

    public class ConditionCusGroupValue
    {
        [Key]
        public int Id { get; set; }
        public  int CondId { get; set; }
        public string Value { get; set; } = "";
        public string ValueName { get; set; } = "";
    }
}
