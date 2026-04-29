using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.TaxGroup;
using BackEndAPI.Models.Unit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace BackEndAPI.Models.ItemMasterData
{
    public class ItemPromotionView
    {
        public int Id { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string? ItemName { get; set; }
        public double? ExchangePoint { get; set; }
        public int? PackingId { get; set; }
        public PackingPromotion? Packing { get; set; }
        public ICollection<ItemPromotionImage>? ITM1 { get; set; }
    }
    public class PackingPromotion
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    } 
    public class ItemPromotionImage
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        [MaxLength(254)] public string? Note { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int? Series { get; set; }
        public string? Currency { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string? ItemName { get; set; }
        [MaxLength(254)] public string? FrgnName { get; set; }
        public int? UgpEntry { get; set; }
        public double? Price { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)] public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)] public string? Updator { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<ITM1>? ITM1 { get; set; }

        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
        public int? IndustryId { get; set; }
        public Industry? Industry { get; set; }
        public int? ItemTypeId { get; set; }
        public ItemType? ItemType { get; set; }
        public int? PackingId { get; set; }
        public Packing? Packing { get; set; }
        public OUGP? OUGP { get; set; }
        public int TaxGroupsId { get; set; }
        public TaxGroups? TaxGroups { get; set; }
        public double? OnHand { get; set; }
        public double? OnOrder { get; set; }
        public bool? IsActive { get; set; }
        [NotMapped]
        public int? ExchangePoint { get; set; }
        [MaxLength(100)] public string? ProductGroupCode { get; set; } = string.Empty;
        [MaxLength(100)] public string? ProductApplicationsCode { get; set; } = string.Empty;
        [MaxLength(100)] public string? ProductQualityLevelCode { get; set; } = string.Empty;

        public int ItemGroupId { get; set; }
        public ProductGroup? ProductGroup { get; set; }
        public ProductApplications? ProductApplications { get; set; }
        public ProductQualityLevel? ProductQualityLevel { get; set; }
        [NotMapped] public double PendingOndHand { get; set; }
        [NotMapped] public double CanBePlacedOnHand { get; set; }
    }
    public class ItemSync
    {
        public string? ItemCode { get; set; }
        public double? Price { get; set; }
        public string? U_UD { get; set; } = string.Empty;
        public string? U_CCL { get; set; } = string.Empty;
        public string? U_NH { get; set; } = string.Empty;
    }

    public class ITM1
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        [MaxLength(254)] public string? Note { get; set; }
        public int ItemId { get; set; }
        [NotMapped] public string? Status { get; set; }
        [JsonIgnore] public Item? Item { get; set; }
    }

    public class OBCD
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string? ItemName { get; set; }
        public int OuomId { get; set; }
        [MaxLength(50)] public string? BcdCode { get; set; }
        [MaxLength(100)] public string? BcdName { get; set; }
        [JsonIgnore] public Item? Item { get; set; }
    }

    public class SyncModel
    {
        [JsonPropertyName("U_NH")] public string? U_NH;
        [JsonPropertyName("U_CCL")] public string? U_CCL;
        [JsonPropertyName("U_UD")] public string? U_UD;
    }

    public class ItemQuery
    {
        public string? search { get; set; }
        public string? brand { get; set; }
        public string? industry { get; set; }
        public bool IsClient { get; set; }
        public string? itemType { get; set; }
        public string? packing { get; set; }
        public int? cardId { get; set; }
        public string? typeDoc { get; set; }
    }
    public class ItemPrice
    {
        public string? ItemCode { get; set; }
        public double? Price { get; set; }
        public string? Currency { get; set; }
    }
    public class ProductGroup
    {
        [Key, MaxLength(100)] public string Code { get; set; } = string.Empty;
        [MaxLength(255)] public string? Name { get; set; } = string.Empty;
    }

    public class ProductApplications
    {
        [Key, MaxLength(100)] public string Code { get; set; } = string.Empty;
        [MaxLength(255)] public string? Name { get; set; } = string.Empty;
    }

    public class ProductQualityLevel
    {
        [Key, MaxLength(100)] public string Code { get; set; } = string.Empty;
        [MaxLength(255)] public string? Name { get; set; } = string.Empty;
    }
    public class ItemPoints
    {
        public string ItemCode { get; set; }
        public int? Points { get; set; }
    } 
}