using BackEndAPI.Models.ARInvoice;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Document
{
    public class DocumentOPDN
    {
        public string? InvoiceCode { get; set; }
        public int CardId { get; set; }
        [MaxLength(50)] public string CardCode { get; set; }
        [MaxLength(254)] public string CardName { get; set; }
        public DateTime DocDate { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public string VATCode { get; set; }
        public double? VATAmount { get; set; }
        public double Total { get; set; }
        public double TotalPayment { get; set; }
        public double PayingAmount { get; set; }
        public string? Note { get; set; }
        public int? ObjType { get; set; } = 20;
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public ICollection<PDN1> ItemDetail { get; set; }
        public PDN12 Address { get; set; }
        public Collection<PDN13> PaymentMethod { get; set; }
    }

    public class PDN1
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string Type { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int ParentItemId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }

        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public double PriceAfterDist { get; set; }
        [MaxLength(10)] public string? VATCode { get; set; }
        public double? VATAmount { get; set; }
        public double LineTotal { get; set; }
        public double? TotalWeight { get; set; }
        public double? GoldAmount { get; set; }
        public double? WeightGold { get; set; }
        public double? StonesAmount { get; set; }
        public double? WeightStones { get; set; }
        public double? WeightOther { get; set; }
        public double? LabourAmount { get; set; }
        public int? InvntryUom { get; set; }
        public int? CountingUom { get; set; }
        public string? Note { get; set; }
        [JsonIgnore] public DocumentOPDN? Document { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Collection<INV16>? Batch { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Collection<INV17>? Serial { get; set; }
    }

    public class PDN12
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string? Address { get; set; }
        [MaxLength(254)] public string? Country { get; set; }
        [MaxLength(50)] public string? City { get; set; }
        [MaxLength(50)] public string? District { get; set; }
        [MaxLength(50)] public string? Ward { get; set; }
        [MaxLength(50)] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
        public string? Note { get; set; }
        [JsonIgnore] public DocumentOPDN? Document { get; set; }
    }

    public class PDN13
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [MaxLength(50)] public string Method { get; set; }
        [MaxLength(254)] public string MethodStr { get; set; }
        public double Amount { get; set; }
        [JsonIgnore] public DocumentOPDN? Document { get; set; }
    }

    public class PDN16
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        [MaxLength(254)] public string BatchNumber { get; set; }
        public double Quantity { get; set; }
        public DateTime ExpDate { get; set; }
        [JsonIgnore] public PDN1? ItemDetail { get; set; }
    }

    public class PDN17
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        [MaxLength(254)] public string SerialNumber { get; set; }
        public DateTime ExpDate { get; set; }
        [JsonIgnore] public PDN1? ItemDetail { get; set; }
    }
}