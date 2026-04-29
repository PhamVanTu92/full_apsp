using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Promotion
{
    public class Voucher
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string VoucherCode { get; set; }
        [MaxLength(254)]
        public string VoucherName { get; set; }
        public string? VoucherDescription { get; set; }
        public double Amount { get; set; }
        public double Denomination { get; set; }
        [MaxLength(2)]
        public string? IsDate {  get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [MaxLength(2)]
        public string? IsExp { get; set; }
        public int? ExpDays { get; set; }

        [MaxLength(2)]
        public string IsMerger { get; set; }
        [MaxLength(2)]
        public string IsAllSystem { get; set; }
        public ICollection<VoucherSystem> VoucherSystem { get; set; }
        [MaxLength(2)]
        public string IsAllCustomer { get; set; }
        public ICollection<VoucherCustomer> VoucherCustomer { get; set; }
        [MaxLength(2)]
        public string IsAllSeller { get; set; }
        public ICollection<VoucherSeller> VoucherSeller { get; set; }
        [MaxLength(2)]
        public string VoucherStatus { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
        public ICollection<VoucherItem> VoucherItem { get; set; }
        public ICollection<VoucherLine> VoucherLine { get; set; }
    }
    public class VoucherLine
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        [MaxLength(50)]
        public string VoucherCode { get; set; }
        public DateTime? ReleaseDate { set; get; }
        public DateTime? ExpDate { set; get; }
        public DateTime? UsingDate { set; get; }
        public double? Amount { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        [JsonIgnore]
        public Voucher? Voucher { get; set; }
    }
    public class VoucherLineAdd
    {
        public int VoucherId { get; set; }
        [MaxLength(50)]
        public string VoucherCode { get; set; }
    }
    public class VoucherItem
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        [MaxLength(2)]
        public string Type { get; set; }
        public int? ItemId { set; get; }
        [MaxLength(50)]
        public string? ItemCode { get; set; }
        [MaxLength(254)]
        public string? ItemName { get; set; }
        public int? ItemGroupId { set; get; }
        [NotMapped]
        public string? Status { get; set; }
        [MaxLength(254)]
        public string? ItmsGrpName { get; set; }
        [JsonIgnore]
        public Voucher? Voucher { get; set; }
    }
    public class VoucherSystem
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int? SystemId { set; get; }
        [MaxLength(50)]
        public string? SystemCode { get; set; }
        [MaxLength(254)]
        public string? SystemName { get; set; }
        [JsonIgnore]
        public Voucher? Voucher { get; set; }
    }
    public class VoucherCustomer
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int? GroupId { set; get; }
        [MaxLength(254)]
        public string? GroupName { get; set; }
        [MaxLength(10)]
        public string? Checked { get; set; }
        [MaxLength(10)]
        public string? PartialChecked { get; set; }
        [JsonIgnore]
        public Voucher? Voucher { get; set; }
    }
    public class VoucherSeller
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int? SellerId { set; get; }
        [MaxLength(50)]
        public string? SellerCode { get; set; }
        [MaxLength(254)]
        public string? SellerName { get; set; }
        [JsonIgnore]
        public Voucher? Voucher { get; set; }
    }
    public class VoucherCodeRule
    {
        [Required]
        public int VoucherId { get; set; }
        public int Quantity { get; set; }
        public int Length { get; set; }
        public string? StartChar { get; set; }
        public string? EndChar { get; set; }
    }
    public class VoucherLineView
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
    }
    public class VoucherListToRelease
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Type { get; set; }

        public DateTime ReleaseDate { get; set; }

        [MaxLength(25)]
        public string PartnerType { get; set; }
        public string PartnerId { get; set; }
        [MaxLength(254)]
        public string PartnerName { get; set; }
        public double? Amount { get; set; }
        [MaxLength(25)]
        public string PaymentMethod { get; set; }
        [MaxLength(254)]
        public string PaymentMethodName { get; set; }

        public int? PaymentAccountId { get; set; }
        [MaxLength(50)]
        public string? PaymentAccountName{ get; set; }
        [MaxLength(254)]
        public string? PaymentAccountUser { get; set; }
        public string Note { get; set; }
        public ICollection<VoucherLineSub> VoucherLine{ get; set; }

    }
    public class VoucherListToCancel
    {
        public int Id { get; set; }
        public ICollection<VoucherLineSub> VoucherLine { get; set; }

    }
    public class VoucherLineSub
    {
        public int VoucherId { get; set; }
    }
    public class VoucherDTO
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string VoucherCode { get; set; }
        [MaxLength(254)]
        public string VoucherName { get; set; }
        public string? VoucherDescription { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public double Denomination { get; set; }
        [MaxLength(2)]
        public string? IsDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [MaxLength(2)]
        public string? IsExp { get; set; }
        public int? ExpDays { get; set; }

        [MaxLength(2)]
        public string IsMerger { get; set; }
        [MaxLength(2)]
        public string IsAllSystem { get; set; }
        public ICollection<VoucherSystem> VoucherSystem { get; set; }
        [MaxLength(2)]
        public string IsAllCustomer { get; set; }
        public ICollection<VoucherCustomer> VoucherCustomer { get; set; }
        [MaxLength(2)]
        public string IsAllSeller { get; set; }
        public ICollection<VoucherSeller> VoucherSeller { get; set; }
        [MaxLength(2)]
        public string VoucherStatus { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
        public ICollection<VoucherItem> VoucherItem { get; set; }
        public ICollection<VoucherLine> VoucherLine { get; set; }
    }
}
