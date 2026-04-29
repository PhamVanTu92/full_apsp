using BackEndAPI.Models.ItemMasterData;

namespace BackEndAPI.Models.ItemGroups
{
    public class OIBTDto
    {
        public int Id { get; set; }
        public string ItmsGrpName { get; set; }
        public List<ItemMasterData.Item> Items { get; set; } = [];
        public List<ConditionItemGroupDto> ConditionItemGroups { get; set; } = [];
        
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        
        public bool IsOneOfThem { get; set; }
        public bool IsSelected { get; set; }

    }
    public class ConditionItemGroupDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        // public string Operator { get; set; } = "";
        public bool IsEqual { get; set; }
        public string TypeCondition { get; set; } = "";
        public List<ConditionItemGroupValue> CondValues { get; set; } = new List<ConditionItemGroupValue>();
    }

    public class ConditionItemGroupValueDto
    {
        public int Id { get; set; }
        public  int CondId { get; set; }
        public string Value { get; set; } = "";
        public string ValueName { get; set; } = "";
    }
}
