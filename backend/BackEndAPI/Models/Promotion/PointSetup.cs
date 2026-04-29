using BackEndAPI.Models.ItemGroups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Promotion
{
    public class PointSetup
    {
        public int Id { get; set; }

        [MaxLength(254)]
        public string Name { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public DateTime? ExtendedToDate { get; set; } 
        public bool IsAllCustomer { get; set; } 
        public int NotifyBeforeDays { get; set; } 

        public bool IsActive { get; set; }
        public string? Note { get; set; }

        public ICollection<PointSetupCustomer> PointSetupCustomer { get; set; } = new List<PointSetupCustomer>();
        public ICollection<PointSetupLine> PointSetupLine { get; set; } = new List<PointSetupLine>();
    }
    public  class PointSetupCustomer
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public PointSetup? PointSetup { get; set; }
        [MaxLength(10)]
        public string Type { get; set; }
        public int CustomerId { get; set; }
        [MaxLength(50)] public string? CustomerCode { get; set; }
        [MaxLength(254)] public string CustomerName { get; set; }
        [NotMapped] public string? Status { get; set; }
    }
    public class PointSetupLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore]  public PointSetup? PointSetup { get; set; }
        public int Point { get; set; }
        // Rule filters
        public ICollection<PointSetupIndustry> Industry { get; set; } = new List<PointSetupIndustry>();
        public ICollection<PointSetupBrand> Brands { get; set; } = new List<PointSetupBrand>();
        public ICollection<PointSetupItemType> ItemType { get; set; } = new List<PointSetupItemType>();
        public ICollection<PointSetupPacking> Packings { get; set; } = new List<PointSetupPacking>();
    }

    // --- Bảng trung gian ---

    public class PointSetupIndustry
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore] public PointSetupLine Line { get; set; }

        public int IndustryId { get; set; }
        public Industry Industry { get; set; }
    }

    public class PointSetupBrand
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore]  public PointSetupLine Line { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; }
    }

    public class PointSetupItemType
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore]  public PointSetupLine Line { get; set; }

        public string ItemType { get; set; }

        public int ItemId { get; set; }
        [MaxLength(50)] public string? ItemCode { get; set; }
        [MaxLength(254)] public string ItemName { get; set; }
    }

    public class PointSetupPacking
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [JsonIgnore]  public PointSetupLine Line { get; set; }

        public int PackingId { get; set; }
        public Packing? Packing { get; set; }
    }
    public class PointSetupCreateDto
    {
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? ExtendedToDate { get; set; }
        public bool IsAllCustomer { get; set; }
        public int NotifyBeforeDays { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }

        public List<PointSetupCustomerCreateDto> Customers { get; set; } = new();
        public List<PointSetupLineCreateDto> Lines { get; set; } = new();
    }
    public class PointSetupCustomerCreateDto
    {
        public string Type { get; set; }   // ví dụ: "Group" hoặc "Individual"
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }
    public class PointSetupLineCreateDto
    {
        public int Point { get; set; }

        public List<int> IndustryIds { get; set; } = new();
        public List<int> BrandIds { get; set; } = new();
        public List<int> PackingIds { get; set; } = new();

        public List<PointSetupItemTypeCreateDto> ItemType { get; set; } = new();
    }
    public class PointSetupItemTypeCreateDto
    {
        public string ItemType { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string ItemName { get; set; }
    }
    public class PointSetupUpdateDto
    {
        public int? Id { get; set; }

        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public DateTime? ExtendedToDate { get; set; }
        public bool IsAllCustomer { get; set; }
        public int NotifyBeforeDays { get; set; }

        public bool IsActive { get; set; }
        public string? Note { get; set; }

        public List<PointSetupCustomerUpdateDto> Customers { get; set; } = new();
        public List<PointSetupLineUpdateDto> Lines { get; set; } = new();
    }

    public class PointSetupCustomerUpdateDto
    {
        public int? Id { get; set; }
        public string Type { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }

    public class PointSetupLineUpdateDto
    {
        public int? Id { get; set; }
        public int Point { get; set; }
        public List<int> IndustryIds { get; set; } = new();
        public List<int> BrandIds { get; set; } = new();
        public List<int> PackingIds { get; set; } = new();

        public List<PointSetupItemTypeUpdateDto> ItemType { get; set; } = new();
    }
    public class PointSetupLineViewData
    {
        public int? Id { get; set; }
        public int Point { get; set; }

        public List<int> IndustryIds { get; set; } = new();
        public List<int> BrandIds { get; set; } = new();
        public List<int> PackingIds { get; set; } = new();
        public List<PointSetupItemTypeViewDto> ItemType { get; set; } = new();
    }
    public class PointSetupItemTypeUpdateDto
    {
        public int? Id { get; set; }
        public string ItemType { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string ItemName { get; set; }
    }
    public class PointSetupViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? ExtendedToDate { get; set; }

        public bool IsAllCustomer { get; set; }
        public int NotifyBeforeDays { get; set; }
        public bool IsActive { get; set; }

        public string? Note { get; set; }

        public List<PointSetupCustomerViewDto> Customers { get; set; } = new();
        public List<PointSetupLineViewDto> Lines { get; set; } = new();
    }

    public class PointSetupCustomerViewDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }
    public class PointSetupLineViewDto
    {
        public int Id { get; set; }
        public int Point { get; set; }

        public List<IndustryViewDto> Industries { get; set; } = new();
        public List<BrandViewDto> Brands { get; set; } = new();
        public List<PackingViewDto> Packings { get; set; } = new();

        public List<PointSetupItemTypeViewDto> ItemType { get; set; } = new();
    }

    // ---- Lookup DTOs ----
    public class IndustryViewDto
    {
        public int Id { get; set; }
        public string? IndustryCode { get; set; }
        public string? IndustryName { get; set; }
    }

    public class BrandViewDto
    {
        public int Id { get; set; }
        public string BrandCode { get; set; }
        public string BrandName { get; set; }
    }

    public class PackingViewDto
    {
        public int Id { get; set; }
        public string PackingCode { get; set; }
        public string PackingName { get; set; }
    }
    public class PointSetupItemTypeViewDto
    {
        public int Id { get; set; }
        public string ItemType { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string ItemName { get; set; }
    }


    public class PointSetupViewData
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? ExtendedToDate { get; set; }

        public bool IsAllCustomer { get; set; }
        public int NotifyBeforeDays { get; set; }
        public bool IsActive { get; set; }

        public string? Note { get; set; }

        public List<PointSetupCustomerViewDto> Customers { get; set; } = new();
        public List<PointSetupLineViewData> Lines { get; set; } = new();
    }
    public class DocLine
    {
        public int CustomerId { get; set; }
        public int IndustryId { get; set; }
        public int BrandId { get; set; }
        public int ItemTypeId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int ItemId { get; set; }
        public int PackingId { get; set; }
        public double Quantity { get; set; }
    }

    public class Redeem
    {
        [Key]
        public int Id { get; set; }
        [NotMapped]
        public string? InvoiceCode { get; set; }

        public string? Status { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(254)]
        public string CustomerName { get; set; }

        [Required]
        public DateTime RedeemDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Note { get; set; }

        public int TotalPointUsed { get; set; }

        public ICollection<RedeemLine> Lines { get; set; } = new List<RedeemLine>();
    }

    public class RedeemLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HeaderId { get; set; }

        [JsonIgnore]
        public Redeem Header { get; set; }

        [Required]
        public int ItemId { get; set; }
        [MaxLength(50)]
        public string ItemCode { get; set; }

        [MaxLength(254)]
        public string ItemName { get; set; }

        [Required]
        public double PointPerUnit { get; set; }

        [Required]
        public double Quantity { get; set; }

        public double TotalPoint => PointPerUnit * Quantity;
    }
    public class RedeemLineDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double PointPerUnit { get; set; }
        public double Quantity { get; set; }
    }

    public class RedeemCreateDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime RedeemDate { get; set; }
        public string? Note { get; set; }
        public List<RedeemLineDto> Lines { get; set; } = new();
    }
    public class CalculatorPoint
    {
        public int DocId { get; set; }
        public int CardId { get; set; }

       public ICollection<CalculatorPointLine> CalculatorPointLine { get; set; }
    }
    public class CalculatorPointLine
    {
        public int ItemId { get; set; }

        public double Quantity { get; set; }

        public double? Point { get; set; }
    }
    public class CalculatorPointReturn
    {
        public int ItemId { get; set; }

        public double Points { get; set; }
        public double Quantity { get; set; }

        public double TotalPoint { get; set; }

        public int PointSetupId { get; set; }
        public string PointSetupName { get; set; }
    }
    public class ReportPoint
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }

        /// <summary>Tổng điểm đã tích trên TẤT CẢ các chương trình.</summary>
        public double TotalPoint { get; set; }
        /// <summary>Tổng điểm đã đổi trên TẤT CẢ các chương trình.</summary>
        public double RedeemPoint { get; set; }
        /// <summary>Tổng điểm còn lại trên TẤT CẢ các chương trình.</summary>
        public double RemainingPoint { get; set; }

        /// <summary>
        /// Chi tiết phân bổ theo từng chương trình (PointSetup).
        /// Mỗi item là 1 chương trình kèm tổng + lịch sử giao dịch riêng.
        /// </summary>
        public ICollection<ReportPointSetupGroup> Setups { get; set; } = new List<ReportPointSetupGroup>();

        /// <summary>
        /// Lịch sử tổng hợp toàn bộ giao dịch (giữ tương thích với client cũ).
        /// Bằng <c>Setups.SelectMany(s =&gt; s.Lines)</c>.
        /// </summary>
        public ICollection<ReportPointLine> ReportPoints { get; set; } = new List<ReportPointLine>();
    }

    /// <summary>
    /// Báo cáo điểm theo từng chương trình (PointSetup) cho 1 khách hàng.
    /// </summary>
    public class ReportPointSetupGroup
    {
        public int PointSetupId { get; set; }
        public string PointSetupName { get; set; } = "";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime? ExtendedToDate { get; set; }

        /// <summary>True nếu chương trình còn hiệu lực (ExpiryDate ≥ today).</summary>
        public bool IsActive { get; set; }

        public double TotalPoint { get; set; }
        public double RedeemPoint { get; set; }
        public double RemainingPoint { get; set; }

        /// <summary>Lịch sử giao dịch CHỈ thuộc chương trình này.</summary>
        public ICollection<ReportPointLine> Lines { get; set; } = new List<ReportPointLine>();
    }

    public class ReportPointLine
    {
        public int PointSetupId { get; set; }
        public string PointSetupName { get; set; } = "";
        public string Type { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime DocDate { get; set; }
        public double Point {  get; set; }
    }
}
