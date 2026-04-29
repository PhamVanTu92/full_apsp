using BackEndAPI.Models.ItemMasterData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.ItemGroups
{
    public class Packing
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        public int? SapId { get; set; }
        public double? Volumn { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }
    }
    public class Industry
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        public IEnumerable<Brand>?  Brands { get; set; }
        [MaxLength(50)]
        public string? SapCode { get; set; }
    }
    public class ItemType
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string? SapCode { get; set; }
    }
    public class Brand
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string? SapCode { get; set; }
        [JsonIgnore]
        public IEnumerable<Industry>? Industrys { get; set; }
        [NotMapped]
        [JsonIgnore]
        public List<Models.Committed.CommittedLineSub>? CommittedLineSub { get; set; }
    }
    public class IndustryDTO
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        public IEnumerable<BrandDTO>? Brands { get; set; }
    }
    public class BrandDTO
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        public IEnumerable<ItemTypeDTO>? ItemTypes { get; set; }
    }
    public class ItemTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AreaLocation
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
    }
    public class IndustryView
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        public IEnumerable<BrandView>? Brands { get; set; }
    }
    public class BrandView
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
        public IEnumerable<ItemTypeView>? ItemTypes { get; set; }
    }
    public class ItemTypeView
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(254)]
        public string Name { get; set; }
    }
}
