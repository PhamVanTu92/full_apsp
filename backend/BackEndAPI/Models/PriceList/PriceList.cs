using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.BusinessPartners;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.PriceList
{
    public class PriceList
    {
        public int Id { get; set; }
        public string PriceListName {get;set;}
        public bool IsAllCustomer { get; set;}
        public bool IsRetail { get; set; }
        public int? CustomerId { get; set;}
        [JsonIgnore]
        public BP? Customer { get; set; }
        public int? CustomerGroupId { get; set;}
        [JsonIgnore]
        public OCRG? CustomerGroup { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime EffectDate { get;set; }
        public DateTime ExpriedDate { get; set; }
        public bool IsActive { get; set; }
        public ICollection<PriceListLine>? PriceListLine { get; set; }
    }
    public class PriceListLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore]
        public PriceList? PriceList { get; set; }
        [MaxLength(50)]
        public string ItemCode { get; set; }
        [MaxLength(254)]
        public string ItemName { get; set; }
        public int PackageId { get; set; }
        [MaxLength(100)]
        public string PackingName { get; set; }
        public double? Price { get; set; }
        [MaxLength(10)]
        public string Currency { get; set; }
    }
    public class PriceListDTO
    {
        public int Id { get; set; }
        public string PriceListName { get; set; }
        public bool IsAllCustomer { get; set; }
        public bool IsRetail { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? CustomerGroupId { get; set; }
        public string? CustomerGroupName { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime EffectDate { get; set; }
        public DateTime ExpriedDate { get; set; }
        public bool IsActive { get; set; }
        public ICollection<PriceListLine>? PriceListLine { get; set; }
    }
}
