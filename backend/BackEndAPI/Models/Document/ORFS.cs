using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Document
{
    public class ORFS
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string InvoiceCode { get; set; }
        public bool InternalCust { get; set; }
        public bool ExternalCust { get; set; }
        public int? CardId { get; set; }
        [MaxLength(50)]
        public string? CardCode { get; set; }
        [MaxLength(255)]
        public string? CardName { get; set; }
        [MaxLength(255)]
        public string? CardType { get; set; }
        public DateTime? DocDate { get; set; }
        public ICollection<RFS1>? RFS1 { get; set; }
        public string? Status { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
    public class RFS1
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int ItemId { get; set; }
        [MaxLength(50)]
        public string? ItemCode { get; set; }
        [MaxLength(254)]
        public string? ItemName { get; set; }
        public double? Quantity { get; set; }
        [MaxLength(10)]
        public string? Result { get; set; }
        [JsonIgnore]
        public ORFS? ORFS { get; set; }
    }
}
