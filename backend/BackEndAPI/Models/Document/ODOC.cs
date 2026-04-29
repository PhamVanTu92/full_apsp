using BackEndAPI.Models.ARInvoice;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemMasterData;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Banks;
using Microsoft.EntityFrameworkCore;
using BackEndAPI.Models.Other;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using Newtonsoft.Json;

namespace BackEndAPI.Models.Document
{
    public class ODOC
    {
        public int Id { get; set; }
        public string? DocType { get; set; }

        [MaxLength(50)] public string? InvoiceCode { get; set; }
        [MaxLength(50)] public string? RefInvoiceCode { get; set; }
        public List<Payment> Payments { get; set; } = [];

        public bool InternalCust { get; set; }
        public bool ExternalCust { get; set; }
        public int? CardId { get; set; }

        public string? Currency { get; set; }

        public int? SapDocEntry { get; set; }

        [MaxLength(50)] public string? CardCode { get; set; }

        [MaxLength(254)] public string? CardName { get; set; }

        public bool IsIncoterm { get; set; }
        public string? IncotermType { get; set; }

        public DateTime? DocDate { get; set; }
        public DateTime? DueDate { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public double? DiscountByPromotion { get; set; }
        public double? DistcountAmountPromotion { get; set; }
        public double? VATAmount { get; set; }
        public double? TotalBeforeVat { get; set; }
        public double? TotalAfterVat { get; set; }
        public double? Total { get; set; }
        public double? Bonus { get; set; }
        public double? BonusAmount { get; set; }
        public double? TotalPayment { get; set; }
        public double? PayingAmount { get; set; }
        public string? Note { get; set; }
        public string? SellerNote { get; set; }
        public int? ObjType { get; set; }
        public int? UserId { get; set; }
        public int? Year { get; set; }

        [MaxLength(25)] public string? SubPeriods { get; set; }

        public int? NoPeriods { get; set; }

        [MaxLength(25)] public string? Status { get; set; } = "DXL";

        public DateTime? DeliveryTime { get; set; }

        [MaxLength(25)] public string? WddStatus { get; set; } = "-";

        public bool? LimitRequire { get; set; }
        public bool? LimitOverDue { get; set; }
        public bool? Other { get; set; }
        public string? Memo { get; set; }

        [NotMapped] public AppUser? Author { get; set; }

        public List<DOC1>? ItemDetail { get; set; }
        public List<DOC2> Promotion { get; set; } = [];
        public List<DOC12>? Address { get; set; }
        public Collection<DOC13>? PaymentMethod { get; set; }
        public Collection<DOC16>? Tracking { get; set; }
        public List<DOC14>? AttachFile { get; set; } = [];
        public List<AttDocument>? AttDocuments { get; set; } = [];
        [NotMapped] public List<CustomerPointLine>? CustomerPointHistory { get; set; } = [];
        public DateTime? ConfirmAt { get; set; }

        [NotMapped]
        [JsonPropertyName("approval")]
        public Approval.Approval? Approvalx
        {
            get { return Approval?.OrderByDescending(e => e.Id).FirstOrDefault(); }
        }

        [JsonIgnore]
        public List<Approval.Approval>? Approval { get; set; }

        public string? ApprovalStatus { get; set; }

        [NotMapped]
        [JsonIgnore]
        public BP? BP { get; set; }

        [Precision(19, 2)] public decimal QuarterlyCommitmentBonus { get; set; } = 0;

        [Precision(19, 2)] public decimal YearCommitmentBonus { get; set; } = 0;

        public bool IsSync { get; set; } = false;
        public bool IsCheck { get; set; } = false;
        public PaymentInfo? PaymentInfo { get; set; }
        [MaxLength(1000)] public string? ReasonForCancellation { get; set; }
        public bool? ZaloConfirmed { get; set; }
        public bool? ZaloCompleted { get; set; }
        public string? ZaloError { get; set; }
        public string? ZaloError1 { get; set; }

        public bool? VnPayStatus { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }

    public class PriceDocCheck
    {
        public int? CardId { get; set; }
        public int? DocId { get; set; }
        public string? DocType { get; set; }
        public double? PayNowAmount { get; set; }
        public List<DOC1DTO>? ItemDetail { get; set; }
        public List<DOC2DTO>? Promotion { get; set; } = [];
        public PaymentInfo? PaymentInfo { get; set; }
    }

    public class PaymentInfo
    {
        public int Id { get; set; }
        public int DocId { get; set; }
        public double VATAmount { get; set; }
        public double PaynowVATAmount { get; set; }             // Tổng thuế thanh toán ngay 
        public double DebtVATAmount { get; set; }               // Tổng thuế thanh toán công nợ
        public double DebtGuaranteeVATAmount { get; set; }      // Tổng thuế thanh toán công nợ bảo lãnh
        public double BonusCommited { get; set; } = 0;          // Tổng tiền chiết khấu cam kết
        public double TotalBeforeVat { get; set; }              // Tổng tiền trước thuể
        public double TotalPayNowBeforeVat { get; set; }        // Tổng tiền thanh toán ngay trước thuế
        public double TotalDebtBeforeVat { get; set; }          // Tổng tiền thanh toán công nợ trước thuế
        public double TotalDebtGuaranteeBeforeVat { get; set; } // Tổng tiền thanh toán công nợ bảo lãnh trước thuế
        public double TotalAfterVat { get; set; }               // Tổng tiền sau thuế
        public double BonusPercent { get; set; }                // Phần trăm bonus
        public double BonusAmount { get; set; }                 // Số tiền bonuss
        public double TotalPayNow { get; set; }                 // Tổng tiền thanh toán ngay
        public double TotalDebt { get; set; }                   // tông tiền thanh toán có nợ tính chấp
        public double TotalDebtGuarantee { get; set; }          // tổng tiền thanh toán nợ bảo lãnh
    }

    public class DiscountUpdate
    {
        public int LineId { get; set; }
        public double Discount { get; set; }
    }

    public class ItemChecking
    {
        public int ItemId { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
    }


    public class DOC1 // Invoice details
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string? Type { get; set; }
        public int? PromotionId { get; set; }
        public int? ItemId { get; set; }
        public bool IsSync { get; set; } = false;
        public int LineId { get; set; }

        [MaxLength(50)] public string? ItemCode { get; set; }

        [MaxLength(254)] public string? ItemName { get; set; }

        public int? BaseId { get; set; } = -1;
        public int? BaseLineId { get; set; } = -1;
        public int? ParentItemId { get; set; }

        [MaxLength(50)] public string? SerialBatchNumber { get; set; }

        [MaxLength(200), NotMapped] public string? PaymentStatus { get; set; }

        public double Quantity { get; set; }
        public double? OpenQty { get; set; }
        public double Price { get; set; } = 0;
        public double PriceAfterDist { get; set; }
        public double? Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public string? DiscountType { get; set; }
        public double? ItemVolume { get; set; }
        [NotMapped]
        public double? Volumn
        {
            get => ItemVolume ?? 0;
            set => ItemVolume = value;
        }
        public double VAT { get; set; }
        public int? VATId { get; set; }

        [MaxLength(25)] public string? VATCode { get; set; }

        [MaxLength(100)] public string? VATName { get; set; }

        public double? VATAmount { get; set; }
        public double? LineTotalBeforeVAT;
        public double  LineTotal;
        public string? Note { get; set; }

        [MaxLength(10)] public string? Result { get; set; }

        public int? OuomId { get; set; }


        [MaxLength(50)] public string? UomCode { get; set; }

        [MaxLength(254)] public string? UomName { get; set; }

        [MaxLength(100)] public string? PaymentMethodCode { get; set; } = "PayNow";

        [JsonIgnore] public Item? Item { get; set; }

        public double? NumInSale { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? DraftId { get; set; }
        public string? Error { get; set; }
    }

    public class CommitedInfo
    {
        [Key] public int Id { get; set; }

        public int DocId { get; set; }
    }

    public class DOC2 //Promotion
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int LineId { get; set; }
        public int[]? ListLineId { get; set; }
        public int? PromotionId { get; set; }

        [NotMapped] public double Price { get; set; }

        public string? PromotionCode { get; set; }
        public string? PromotionName { get; set; }
        public string? PromotionDesc { get; set; }
        public bool IsOtherPromotion { get; set; }
        public bool IsOtherDist { get; set; }
        public bool IsOtherPay { get; set; }

        public bool HasException { get; set; }
        public bool IsOtherPromotionExc { get; set; }
        public bool IsOtherDistExc { get; set; }

        public bool IsOtherPayExc { get; set; }
        public string? ItemGroup { get; set; }
        public int? ItemId { get; set; }
        public bool AddAccumulate { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? PackingId { get; set; }
        public string? PackingName { get; set; }
        public double? Volumn { get; set; }
        public double? QuantityAdd { get; set; }
        public string? Note { get; set; }
        public double? Discount { get; set; }
        public string? DiscountType { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }
    }

    public class DOC3 //Coupon
    {
        public string Id { get; set; }
        public int FatherId { get; set; }
        public int CouponId { get; set; }
        public double? CouponAmount { get; set; } = 0;
        public double? CouponDiscount { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }
    }

    public class DOC4 //Voucher
    {
        public string Id { get; set; }
        public int FatherId { get; set; }
        public int VoucherId { get; set; }
        public double? VoucherAmount { get; set; } = 0;
        public double? VoucherDiscount { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }
    }

    public class DOC12
    {
        public int Id { get; set; }
        public int FatherId { get; set; }

        [MaxLength(1000)] public string? Address { get; set; }

        public int? LocationId { get; set; }

        [MaxLength(254)] public string? LocationName { get; set; }

        public int? AreaId { get; set; }
        [MaxLength(25)] public string? Type { get; set; } = "S";
        [MaxLength(254)] public string? AreaName { get; set; }
        [MaxLength(50)] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(50)] public string? VehiclePlate { get; set; }
        [MaxLength(50)] public string? CCCD { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
    }

    public class DOC13
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int PaymentMethodID { get; set; }

        [MaxLength(50)] public string PaymentMethodCode { get; set; } = "";

        [MaxLength(254)] public string PaymentMethodName { get; set; } = "";

        [JsonIgnore] public ODOC? Document { get; set; }
    }

    public class AttDocument
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }

        public DateTime? UploadFileAt { get; set; } = DateTime.UtcNow;
        public int FatherId { get; set; }
        public int? AuthorId { get; set; }
        public string? AuthorName => Author?.FullName;

        [JsonIgnore] public AppUser? Author { get; set; }
    }

    public class DOC14
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        [MaxLength(254)] public string? Note { get; set; }

        [MaxLength(254)] public string? Memo { get; set; }

        public int FatherId { get; set; }

        [NotMapped] public string? Status { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }

        public DateTime? UploadFileAt { get; set; } = DateTime.UtcNow;
        public int? AuthorId { get; set; }
        public string? AuthorName => Author?.FullName;

        [JsonIgnore] public AppUser? Author { get; set; }
    }

    public class DOC16
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string Type { get; set; }
        public int DocID { get; set; }
        public string DocCode { get; set; }
        public double Volumn { get; set; }
        public double Bonus { get; set; }

        [JsonIgnore] public ODOC? Document { get; set; }
    }

    public class DOCUpdate
    {
        [JsonProperty("document")] public string? document { get; set; }

        [JsonProperty("attachFile")] public List<IFormFile>? attachFile { get; set; }

        public int UserId { get; set; }
    }

    public class DOCAttach
    {
        [JsonProperty("attachFile")] public List<IFormFile>? attachFile { get; set; }
    }


    public class DOC2DTO
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int LineId { get; set; }
        public int[] ListLineId { get; set; }
        public int? PromotionId { get; set; }

        [NotMapped] public double Price { get; set; }

        public string? PromotionCode { get; set; }
        public string? PromotionName { get; set; }
        public string? PromotionDesc { get; set; }
        public bool? IsOtherPromotion { get; set; }
        public bool? IsOtherDist { get; set; }
        public bool? IsOtherPay { get; set; }

        public bool? HasException { get; set; }
        public bool? IsOtherPromotionExc { get; set; }
        public bool? IsOtherDistExc { get; set; }

        public bool? IsOtherPayExc { get; set; }
        public string? ItemGroup { get; set; }
        public int? ItemId { get; set; }
        public bool? AddAccumulate { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? PackingId { get; set; }
        public string? PackingName { get; set; }
        public double? Volumn { get; set; }
        public double? QuantityAdd { get; set; }
        public string? Note { get; set; }
        public double? Discount { get; set; }
        public string? DiscountType { get; set; }
    }

    public class DOC1DTO // Invoice details
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string? Type { get; set; }
        public int? PromotionId { get; set; }
        public int? ItemId { get; set; }

        public int LineId { get; set; }

        [MaxLength(50)] public string? ItemCode { get; set; }

        [MaxLength(254)] public string? ItemName { get; set; }

        public int? BaseId { get; set; } = -1;
        public int? BaseObj { get; set; } = -1;
        public int? ParentItemId { get; set; }

        [MaxLength(50)] public string? SerialBatchNumber { get; set; }

        public double Quantity { get; set; }
        public double Price { get; set; } = 0;
        public double PriceAfterDist { get; set; }
        public double Discount { get; set; }
        public double? DistcountAmount { get; set; }
        public string? DiscountType { get; set; }
        public double? ItemVolume { get; set; }
        public double VAT { get; set; }
        public int? VATId { get; set; }

        [MaxLength(25)] public string? VATCode { get; set; }

        [MaxLength(100)] public string? VATName { get; set; }

        public double? VATAmount { get; set; }
        public double? LineTotalBeforeVAT;
        public double  LineTotal;
        public string? Note { get; set; }

        [MaxLength(10)] public string? Result { get; set; }

        public int? OuomId { get; set; }

        [MaxLength(50)] public string? UomCode { get; set; }

        [MaxLength(254)] public string? UomName { get; set; }

        [MaxLength(100)] public string? PaymentMethodCode { get; set; } = "PayNow";

        public double? NumInSale { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ODOCDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public int CustomerGroupId { get; set; }
        public DateTime DocDate { get; set; }
        public ICollection<DOC1PointDto>? lines { get; set; }
    }

    public class DOC1PointDto
    {
        public int ItemId { get; set; }
        public double Quantity { get; set; }

        public int PackingId { get; set; }
        public int ItemTypeId { get; set; }
        public int BrandId { get; set; }
        public int IndustryId { get; set; }
    }

    public class SyncInvoiceErrorDto
    {
        public int Id { get; set; }
        public string InvoiceCode { get; set; }
        public int ObjType { get; set; }
        public string CardCode { get; set; }
        public int CardId { get; set; }
        public string CardName { get; set; }
        public string Error { get; set; } // Ghép từ STRING_AGG(Error)
    }

    public class DOC1OrderReturnDto
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public int? ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }

        [MaxLength(254)] public string? ItemName { get; set; }
        public double Quantity { get; set; }
        public double? Price { get; set; }
        public int? OuomId { get; set; }

        [MaxLength(50)] public string? UomCode { get; set; }

        [MaxLength(254)] public string? UomName { get; set; }

        [JsonIgnore] public OrderReturnDto? Document { get; set; }
    }
    public class OrderReturnDto
    {
        public int Id { get; set; }
        public string? DocType { get; set; }

        [MaxLength(50)] public string? InvoiceCode { get; set; }
        public int? CardId { get; set; }

        public string? Currency { get; set; }

        public int? SapDocEntry { get; set; }

        [MaxLength(50)] public string? CardCode { get; set; }

        [MaxLength(254)] public string? CardName { get; set; }
        public DateTime? DocDate { get; set; }
        public List<DOC1OrderReturnDto> ItemDetails { get; set; } = [];
    }

    public class OrderReturnDetails
    {
        public int? BaseId { get; set; } = -1;
        public int? BaseLineId { get; set; } = -1;
        public string? Type { get; set; }
        public int? ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }

        [MaxLength(254)] public string? ItemName { get; set; }
        public double Quantity { get; set; }
        public double? Price { get; set; }
        public int? OuomId { get; set; }

        [MaxLength(50)] public string? UomCode { get; set; }

        [MaxLength(254)] public string? UomName { get; set; }

        [JsonIgnore] public OrderReturnDto? Document { get; set; }
    }
    public class OrderReturn
    {
        public int Id { get; set; }
        public int ObjType { get; set; }
        public string? DocType { get; set; }

        [MaxLength(50)] public string? RefInvoiceCode { get; set; }
        public int? CardId { get; set; }

        public string? Currency { get; set; }

        [MaxLength(50)] public string? CardCode { get; set; }

        [MaxLength(254)] public string? CardName { get; set; }
        public DateTime? DocDate { get; set; }
        public int? UserId { get; set; }
        [NotMapped]
        [JsonProperty("document")] public string? document { get; set; }
        [NotMapped]
        [JsonProperty("attachFile")] public List<IFormFile>? attachFile { get; set; }
        public List<OrderReturnDetails> ItemDetails { get; set; } = [];
    }
}