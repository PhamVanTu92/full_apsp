using BackEndAPI.Models.ItemMasterData;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.ARInvoice
{
    public class OINV
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string? InvoiceCode { get; set; }
        public int CardId { get; set; }
        [MaxLength(50)]
        public string CardCode { get; set; }
        [MaxLength(254)]
        public string CardName { get; set; }
        public DateTime DocDate { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public double?  DiscountByPromotion { get; set; }
        public double? DistcountAmountPromotion { get; set; }
        public string VATCode { get; set; }
        public double? VATAmount { get; set; }
        public double Total { get; set; }
        public double TotalPayment { get; set; }
        public double PayingAmount { get; set; }
        public string? Note { get; set; }
        public int ObjType { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public ICollection<INV1> INV1 { get; set; }
        public INV12 INV12 { get; set; }
        public Collection<INV13> INV13 { get; set; }
        //public Collection<INV3> INV3 { get; set; }
        //public Collection<INV1> INV4 { get; set; }
        
        

    }
    public class INV1 // Invoice details
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string Type { get; set; }
        public int PromotionId { get; set; }
        public int ItemId {  get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int ParentItemId { get; set; }
        [MaxLength(50)]
        public string SerialNumbers { get; set; }
        public double Quantity {  get; set; }
        public double Price { get; set; }
        public double PriceAfterDist { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        [MaxLength(10)]
        public string VATCode { get; set; }
        public double? VATAmount { get; set; }
        public double LineTotal {  get; set; }
        public double TotalWeight { get; set; }
        public double GoldAmount { get; set; }
        public double WeightGold { get; set; }
        public double StonesAmount { get; set; }
        public double WeightStones { get; set; }
        public double WeightOther { get; set; }
        public double LabourAmount { get; set; }
        public int InvntryUom { get; set; }
        public int CountingUom { get; set; }
        public string? Note { get; set; }
        [JsonIgnore]
        public OINV? OINV { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Collection<INV16>? Batch { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Collection<INV17>? Serial { get; set; }
    }
    public class INV2 //Promotion
    {
        public string Id { get; set; }
        public int FatherId { get; set; }
        public int PromotionId { get; set; }
        public double? PromotionAmount { get; set; } = 0;
        public double? PromotionDiscount { get; set; } = 0;
        public  DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonIgnore]
        public OINV? OINV { get; set; }
    }
    public class INV3 //Coupon
    {
        public string Id { get; set; }
        public int FatherId { get; set; }
        public int CouponId { get; set; }
        public double? CouponAmount { get; set; } = 0;
        public double? CouponDiscount { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonIgnore]
        public OINV? OINV { get; set; }
    }
    public class INV4 //Voucher
    {
        public string Id { get; set; }
        public int FatherId { get; set; }
        public int VoucherId { get; set; }
        public double? VoucherAmount { get; set; } = 0;
        public double? VoucherDiscount { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonIgnore]
        public OINV? OINV { get; set; }
    }
    public class INV12 
    { 
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string? Address { get; set; }
        [MaxLength(254)]
        public string? Country { get; set; }
        [MaxLength(50)]
        public string? City { get; set; }
        [MaxLength(50)]
        public string? District { get; set; }
        [MaxLength(50)]
        public string? Ward { get; set; }
        [MaxLength(50)]
        public string? Email { get; set; }
        [MaxLength(50)]
        public string? Phone { get; set; }
        [MaxLength(100)]
        public string? Person { get; set; }
        public string? Note { get; set; }
        [JsonIgnore]
        public OINV? OINV { get; set; }
    }
    public class INV13
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [MaxLength(50)]
        public string? Method { get; set; }
        [MaxLength(254)]
        public string? MethodStr { get; set; }
        public double Amount { get; set; }
        [JsonIgnore]
        public OINV? OINV { get; set; }
    }
    public class INV16
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        [MaxLength(254)]
        public string? BatchNumber { get; set; }
        public double Quantity { get; set; }
        public DateTime? ExpDate { get; set; }
        [JsonIgnore]
        public INV1? INV1 { get; set; }
    }
    public class INV17
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        [MaxLength(254)]
        public string? SerialNumber { get; set; }
        public DateTime? ExpDate { get; set; }
        [JsonIgnore]
        public INV1? INV1 { get; set; }
    }
}
