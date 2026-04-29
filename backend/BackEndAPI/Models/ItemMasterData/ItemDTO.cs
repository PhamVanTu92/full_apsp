using BackEndAPI.Models.ItemGroups;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.ItemMasterData
{
    public class CCL
    {
        public string Code { get; set; }
        public string? Name { get; set; }
    }
    public class ItemMaster
    {
        public string ItemCode { get; set; }
        public string? ItemName { get; set; }
        public double? Price { get; set; }
        public double? Vat { get; set; }
        public string? Industry { get; set; }
        public string? Brand { get; set; }
        public string? ItemType { get; set; }
        public string? UoM { get; set; }
        public string? NH { get; set; }
        public string? UD { get; set; }
        public string? CCL { get; set; }
        public string? validFor { get; set; }
        public string? Currency { get; set; }
    }
    public class ItemDTO
    {
        public int Id { get; set; }
        public int? Series { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? FrgnName { get; set; }
        public int? UgpEntry { get; set; }
        public double? Price { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public int? BrandId { get; set; }
        // public Brand? Brand { get; set; }
        public int? IndustryId { get; set; }
        // public Industry? Industry { get; set; }
        public int? ItemTypeId { get; set; }
        public ItemType? ItemType { get; set; }
        public int? PackingId { get; set; }
        public Packing? Packing { get; set; }
        public int TaxGroupsId { get; set; }
        public double? OnHand { get; set; }
        public double? OnOrder { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ItemListView
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? ItmsGrpCode { get; set; }
        public double? Onhand { get; set; }
        public double? IsCommited { get; set; }
        public double? Price { get; set; }
        public string? ManBtchNum { get; set; }
        public string? ManSerNum { get; set; }
        public double? IsPriceCurrent { get; set; }
        public string? SerialBatchNumber { get; set; }
        public double? TotalWeight { get; set; }
        public double? GoldAmount { get; set; }
        public double? WeightGold { get; set; }
        public double? StonesAmount { get; set; }
        public double? WeightStones { get; set; }
        public double? WeightOther { get; set; }
        public double? LabourAmount { get; set; }
        public DateTime? ExpDate { get; set; }
        public string? GoldContent { get; set; }
        public string? StonesType { get; set; }
    }

    public class ItemPromotion
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? FrgnName { get; set; }
        public int PackingId { get; set; }
        public string PackingName { get; set; }
    }
}