using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Promotion
{
    public class Coupon
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string CouponCode { get; set; }
        [MaxLength(254)]
        public string CouponName { get; set; }
        public string? CouponDescription { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; }
        public double MaxDiscountAmount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [MaxLength(2)]
        public string CouponStatus { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
        public ICollection<CouponItem> CouponItem { get; set; }
        public ICollection<CouponLine> CouponLine { get; set; }
    }
    public class CouponLine
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        [MaxLength(50)]
        public string CouponCode { get; set; }
        public DateTime? ReleaseDate { set; get; }
        public DateTime? UsingDate { set; get; }
        public double? Amount { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        [JsonIgnore]
        public Coupon? Coupon { get; set; }
    }
    public class CouponItem
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        [MaxLength(2)]
        public string Type { get; set; }
        public int? ItemId { set; get; }
        [MaxLength(50)]
        public string? ItemCode { get; set; }
        [MaxLength(254)]
        public string? ItemName { get; set; }
        public int? ItemGroupId { set; get; }
        [MaxLength(254)]
        public string? ItmsGrpName { get; set; }
        [NotMapped]
        public string? Status { get; set; }
        [JsonIgnore]
        public Coupon? Coupon { get; set; }
    }
    public class CouponCodeRule
    {
        [Required]
        public int CouponId { get; set; }
        public int Quantity { get; set; }
        public int Length { get; set; }
        public string? StartChar { get; set; }
        public string? EndChar { get; set; }
    }
    public class CouponView
    {
        public int Id { get; set; }
        [MaxLength(254)]
        public string CouponName { get; set; }
        public string? CouponDescription { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; }
        public double MaxDiscountAmount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [MaxLength(2)]
        public string CouponStatus { get; set; }
        public string? Updator { get; set; }
        public ICollection<CouponItem> CouponItem { get; set; }
    }

    public class CouponLineView
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
    }
    public class CouponDTO
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string CouponCode { get; set; }
        [MaxLength(254)]
        public string CouponName { get; set; }
        public string? CouponDescription { get; set; }
        public double Amount { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public double MaxDiscountAmount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [MaxLength(2)]
        public string CouponStatus { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
        public ICollection<CouponItem> CouponItem { get; set; }
        public ICollection<CouponLine> CouponLine { get; set; }
    }
}
