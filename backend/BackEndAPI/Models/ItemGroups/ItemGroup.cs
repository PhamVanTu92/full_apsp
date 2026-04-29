using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndAPI.Models.ItemMasterData;
using System.Text.Json.Serialization;
using BackEndAPI.Models.BPGroups;

namespace BackEndAPI.Models.ItemGroups
{
    public class OIBT
    {
        public int Id { get; set; }
        public string ItmsGrpName { get; set; }
        public List<ItemMasterData.Item> Items { get; set; } = [];
        
        [NotMapped]
        public List<int> ItemIds { get; set; } = new List<int>();

        public List<ConditionItemGroup> ConditionItemGroups { get; set; } = [];
        
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        
        public bool IsOneOfThem { get; set; }
        public bool IsSelected { get; set; }

    }
    public class ConditionItemGroup
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }
        // public string Operator { get; set; } = "";
        public bool IsEqual { get; set; }
        public string TypeCondition { get; set; } = "";
        public List<ConditionItemGroupValue> CondValues { get; set; } = new List<ConditionItemGroupValue>();
        [NotMapped]
        public List<string> Values { get; set; }
        
    }

    public class ConditionItemGroupValue
    {
        [Key]
        public int Id { get; set; }
        public  int CondId { get; set; }
        public string Value { get; set; } = "";
        public string ValueName { get; set; } = "";
    }
}
