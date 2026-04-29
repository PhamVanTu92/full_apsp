using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Document
{
    public class OPOR
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string? InvoiceCode { get; set; }
        public int? CardId { get; set; }
        [MaxLength(50)]
        public string? CardCode { get; set; }
        [MaxLength(254)]
        public string? CardName { get; set; }
        public DateTime? DocDate { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public double? VATAmount { get; set; }
        public double? Total { get; set; }
        public string? Note { get; set; }
        public int? UserId { get; set; }
        public ICollection<POR1>? ItemDetail { get; set; }
    }
    public class POR1 // Invoice details
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string? Type { get; set; }
        public int? ItemId { get; set; }
        [MaxLength(50)]
        public string? ItemCode { get; set; }
        [MaxLength(254)]
        public string? ItemName { get; set; }
        public double? Quantity { get; set; }
        public double? Price { get; set; }
        public double? PriceAfterDist { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }

        public int? VATId { get; set; }
        [MaxLength(25)]
        public string? VATCode { get; set; }
        [MaxLength(100)]
        public string? VATName { get; set; }

        public double? VATAmount { get; set; }
        public double? LineTotal { get; set; }
        public string? Note { get; set; }
        public int? OuomId { get; set; }
        [MaxLength(50)]
        public string? UomCode { get; set; }
        [MaxLength(254)]
        public string? UomName { get; set; }

        public double? NumInSale { get; set; }
        [JsonIgnore]
        public OPOR? Document { get; set; }
    }
}
