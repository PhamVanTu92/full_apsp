using BackEndAPI.Models.ItemGroups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static BackEndAPI.Models.Promotion.PromotionCustomer;

namespace BackEndAPI.Models.Promotion
{
    public class Promotion
    {
        public int Id { get; set; }
        [MaxLength(50)] public string? PromotionCode { get; set; }
        [MaxLength(254)] public string? PromotionName { get; set; }
        public string? PromotionDescription { get; set; }

        [Column(TypeName = "date")] public DateTime FromDate { get; set; }
        [Column(TypeName = "date")] public DateTime ToDate { get; set; }
        public string? PromotionMonths { get; set; }
        public bool IsBirthday { get; set; }
        public int BeforeDay { get; set; }
        public int AfterDay { get; set; }

        public bool IsPayNow { get; set; }
        public bool IsCredit { get; set; }
        public bool IsCreditGuarantee { get; set; }
        public bool IsAllCustomer { get; set; }
        public bool IsIgnore { get; set; }
        [MaxLength(2)] public string? PromotionStatus { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)] public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)] public string? Updator { get; set; }
        [MaxLength(50)] public string? Status { get; set; }
        public string? Note { get; set; }
        public bool IsOtherPromotion { get; set; }
        public bool IsOtherDist { get; set; }
        public bool IsOtherPay { get; set; }

        public bool HasException { get; set; }
        public bool IsOtherPromotionExc { get; set; }
        public bool IsOtherDistExc { get; set; }

        public bool IsOtherPayExc { get; set; }

        // 1 - Invoice(Hóa đơn) , 2 - Product(Hàng hóa), 3 - Invoice&Product(Tổng hợp)
        public int PromotionType { get; set; }

        public ICollection<PromotionLine> PromotionLine { get; set; }

        //public PromotionLineEx PromotionLineEx { get; set; }
        public ICollection<PromotionCustomer>? PromotionCustomer { get; set; }
        public ICollection<PromotionBrand>? PromotionBrand { get; set; }
        public ICollection<PromotionIndustry>? PromotionIndustry { get; set; }
    }

    public class PromotionBrand
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public Promotion? Promotion { get; set; }
        public int BrandId { get; set; }
        [MaxLength(254)] public string BrandName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionIndustry
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public Promotion? Promotion { get; set; }
        public int IndustryId { get; set; }
        [MaxLength(254)] public string IndustryName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionCustomer
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public Promotion? Promotion { get; set; }

        [MaxLength(10)]
        // G - Nhóm khách hàng , C- Khách hàng
        public string Type { get; set; }

        public int CustomerId { get; set; }
        [MaxLength(50)] public string? CustomerCode { get; set; }
        [MaxLength(254)] public string CustomerName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }

        [JsonIgnore] public Promotion? Promotion { get; set; }

        // AND - Và , OR - Hoặc
        [MaxLength(10)] public string Cond { get; set; }

        // 1 - Mua hàng tặng hàng , 2 - Giảm giá hóa đơn , 3 - Giảm giá hang
        public int SubType { get; set; }
        public bool AddAccumulate { get; set; }
        public bool HasException { get; set; }
        [NotMapped] public string? Status { get; set; }
        public ICollection<PromotionLineSub> PromotionLineSub { get; set; }
    }

    public class PromotionLineSub
    {
        public int Id { get; set; }

        public int FatherId { get; set; }

        // 1 - Theo số lượng mua, 2 - Theo sản lượng
        public int? FollowBy { get; set; }
        [JsonIgnore] public PromotionLine? PromotionLine { get; set; }
        public int Quantity { get; set; }
        public bool IncludesP { get; set; }
        [MaxLength(2)]
        // Q- Số lượng , R - Tỷ lệ
        public string? AddType { get; set; }

        // Tỷ lệ : Số lượng mua
        public int? AddBuy { get; set; }

        // Tỷ lệ : Số lượng tặng, Số lượng: Số lượng tặng
        public int AddQty { get; set; }

        public bool IsSameType { get; set; }

        // MIN - Giá trị thấp nhât, MAX: Giá trị lớn nhất, OPT: Lựa chọn
        [MaxLength(3)] public string? AddValueType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? MinVolumn { get; set; }
        public double? Discount { get; set; }

        [MaxLength(2)]
        // M - Tiền , P - %
        public string? DiscountType { get; set; }

        [MaxLength(2)]
        // P - Giá niêm yết, V - Giá VAT
        public string? PriceType { get; set; }

        [NotMapped] public string? Status { get; set; }
        public ICollection<PromotionLineSubSub>? PromotionLineSubSub { get; set; }
        public ICollection<PromotionItemBuy>? PromotionItemBuy { get; set; }
        public ICollection<PromotionUnit>? PromotionUnit { get; set; }
    }

    public class PromotionItemBuy
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public PromotionLineSub? PromotionLineSub { get; set; }

        [MaxLength(2)]
        // I - Hàng hóa, sản phẩm , G - Loại sản phẩm
        public string ItemType { get; set; }

        public int ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string ItemName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionUnit
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public PromotionLineSub? PromotionLineSub { get; set; }
        public int UomId { get; set; }
        [MaxLength(50)] public string UomName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionLineSubSub
    {
        public int Id { get; set; }
        public int FatherId { get; set; }

        [JsonIgnore] public PromotionLineSub? PromotionLineSub { get; set; }

        // AND - Và , OR - Hoặc
        [MaxLength(10)] public string Cond { get; set; }
        public int Quantity { get; set; }
        public int InGroup { get; set; }
        [NotMapped] public string? Status { get; set; }
        public ICollection<PromotionSubItemAdd> PromotionSubItemAdd { get; set; }
        public ICollection<PromotionSubUnit> PromotionSubUnit { get; set; }
    }

    public class PromotionSubItemAdd
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public PromotionLineSubSub? PromotionLineSubSub { get; set; }

        [MaxLength(2)]
        // I - Hàng hóa, sản phẩm , G - Loại sản phẩm
        public string ItemType { get; set; }

        public int ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string ItemName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionSubUnit
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public PromotionLineSubSub? PromotionLineSubSub { get; set; }
        public int UomId { get; set; }
        public string UomName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }

    public class PromotionOrder
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CardId { get; set; }
        public List<PromotionOrderLine> PromotionOrderLine { get; set; }
    }

    public class PromotionOrderLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        public double Quantity { get; set; }
        public int PromotionId { get; set; }
        public string? PromotionCode { get; set; }
        public string? PromotionName { get; set; }
        public string? PromotionDesc { get; set; }
        // bo? cham' hoi? di nhe' anh
        public bool IsOtherPromotion { get; set; }
        public bool IsOtherDist { get; set; }
        public bool IsOtherPay { get; set; }
        public bool HasException { get; set; }
        public bool IsOtherPromotionExc { get; set; }
        public bool IsOtherDistExc { get; set; }
        public bool IsOtherPayExc { get; set; }
        [JsonIgnore] public PromotionOrder? PromotionOrder { get; set; }
        public List<PromotionOrderLineSub>? PromotionOrderLineSub { get; set; }
    }

    public class PromotionOrderLineSub
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public string Cond { get; set; }
        public int InGroup { get; set; }
        public int LineId { get; set; }
        public int[] ListLineId { get; set; }
        public string? ItemGroup { get; set; }
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public bool? AddAccumulate { get; set; }
        public string? ItemName { get; set; }
        public int PackingId { get; set; }
        public string PackingName { get; set; }
        public double? Volumn { get; set; }
        public double? QuantityAdd { get; set; }
        public string? Note { get; set; }
        public double? Discount { get; set; }
        public string? DiscountType { get; set; }
        public string? PriceType { get; set; }
        [JsonIgnore] public PromotionOrderLine? PromotionOrderLine { get; set; }
    }

    public class PromotionParam
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CardId { get; set; }
        public string? DocType { get; set; }
        public List<PromotionParamLine> PromotionParamLine { get; set; }
    }

    public class PromotionParamLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int LineId { get; set; }
        public int ItemId { get; set; }
        public double Quantity { get; set; }
        public string? PayMethod { get; set; }
        [JsonIgnore] public double QuantityRef { get; set; }
        [JsonIgnore] public PromotionOrder? PromotionOrder { get; set; }
    }
}