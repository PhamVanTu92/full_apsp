using BackEndAPI.Models.Account;
using BackEndAPI.Models.Document;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.BPGroups;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.LocationModels;

namespace BackEndAPI.Models.BusinessPartners
{
    public class BPSize
    {
        public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; } = "";
    }
    public class OWDDS
    {
        public string OrderCode { get; set; }
    }
    public class BPAreaView
    {
        public int Id { get; set; }
        public string BPAreaName { get; set; } = "";
        public int? ParentId { get; set; }
        public BPArea? ParentGroup { get; set; }
        public ICollection<BPArea>? ChildPGroups { get; set; } = new List<BPArea>();
    }

    public class BPArea
    {
        public int Id { get; set; }

        public string BPAreaName { get; set; } = "";

        public int? ParentId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BPArea? Parent { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<BPArea>? Child { get; set; }

        public bool IsCircularReference(BPArea parent)
        {
            var current = parent;
            while (current != null)
            {
                if (current.Id == Id)
                {
                    return true;
                }

                current = current.Parent;
            }

            return false;
        }
    }

    public class BPView
    {
        [MaxLength(50)] public string? CardCode { get; set; }
        [MaxLength(254)] public string? CardName { get; set; }
        [MaxLength(254)] public string? FrgnName { get; set; }
        [MaxLength(25)] public string? LicTradNum { get; set; }
        public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
        public double CNTC { get; set; }
        public double CNBL { get; set; }
        public string? InvoiceCode { get; set; }
        public string? Payment { get; set; }
        public double PaidAmount { get; set; }
    }
    public class BPSyncView
    {
        public int SYS_CHANGE_VERSION  { get; set; }
        [MaxLength(50)] public string? CardCode { get; set; }
        [MaxLength(254)] public string? CardName { get; set; }
        [MaxLength(254)] public string? FrgnName { get; set; }
        [MaxLength(25)] public string? LicTradNum { get; set; }
        public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
        public double? CNTC { get; set; }
        public double? CNBL { get; set; }
        public int? Times { get; set; }
        public int? TimesBL { get; set; }
        public string? InvoiceCode { get; set; }
        public string? Payment { get; set; }
        public double? PaidAmount { get; set; }
        public double? DocTotal { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
    }
    public class BPGroup
    {
        public int ID { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
    public class BPView1
    {
        [MaxLength(50)] public string? CardCode { get; set; }
        [MaxLength(254)] public string? CardName { get; set; }
        [MaxLength(254)] public string? FrgnName { get; set; }
        [MaxLength(25)] public string? LicTradNum { get; set; }
        public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
        public double CNTC { get; set; }
        public double CNBL { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }
    }
    public class BP
    {
        [Key] // Đánh dấu là khóa chính
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? Series { get; set; }
        [MaxLength(50)] public string? CardCode { get; set; }
        [MaxLength(254)] public string? CardName { get; set; }
        [MaxLength(254)] public string? FrgnName { get; set; }
        [MaxLength(50)] public string? CardType { get; set; }
        [MaxLength(25)] public string? LicTradNum { get; set; }
        public bool IsBusinessHouse { get; set; }
        public bool IsInterCom { get; set; }
        public string? Avatar { get; set; }
        [MaxLength(50)] public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(254)] public string? LocationName { get; set; }
        public int? AreaId { get; set; }
        [MaxLength(254)] public string? AreaName { get; set; }
        [MaxLength(254)] public string? Address { get; set; }

        [MaxLength(50)] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
        public string? Note { get; set; }
        [MaxLength(50)] public string? Status { get; set; }
        [MaxLength(50)] public string? RStatus { get; set; } = "D";

        public string? PortalRegistrationStatus { get; set; } = "N";
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string? Creator { get; set; }
        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)] public string? Updator { get; set; }
        public List<CRD1>? CRD1 { get; set; } = new List<CRD1>();
        public ICollection<CRD2>? CRD2 { get; set; }
        public ICollection<CRD3>? CRD3 { get; set; }
        public ICollection<CRD4>? CRD4 { get; set; }
        public ICollection<CRD5>? CRD5 { get; set; }
        public List<CRD6>? CRD6 { get; set; } = new List<CRD6>();
        [JsonIgnore] public ICollection<ODOC>? ODOC { get; set; }
        public bool? IsAllArea { get; set; }
        public string? Area { get; set; }

        public bool? IsAllBrand { get; set; }
        public string? Brand { get; set; }
        public bool? IsAllItemType { get; set; }
        public string? ItemType { get; set; }

        public bool? IsAllIndustry { get; set; }
        public string? Industry { get; set; }
        public bool? IsAllPacking { get; set; }
        public string? Packing { get; set; }

        public bool? IsAllBPArea { get; set; }
        public string? BPArea { get; set; }
        public bool? IsAllBPSize { get; set; }
        public string? BPSize { get; set; }
        public AppUser? UserInfo { get; set; }

        public List<BpClassify>? Classify { get; set; } = new List<BpClassify>();
        public List<Committed.Committed>? Commiteds { get; set; }
        [NotMapped] public Committed.Committed? CurrentCommited { get; set; }
        [NotMapped] public ICollection<CustomerPointCycleDTO>? CustomerPoints { get; set; }
        public List<OCRG>? Groups { get; set; }
        public int? SaleId { get; set; }
        public AppUser? SaleStaff { get; set; } 
        
        public string? SapCardCode { get; set; } 
        
        public int? SapGroupCode { get; set; }
    }
    public class PaymentRule
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public double? PromotionTax { get; set; }
        public double? BonusPaynow { get; set; }
        public double? BonusVolumn { get; set; }
    }
    public class PaymentRuleView
    {
        public int Id { get; set; }
        public double? PromotionTax { get; set; }
        public double? BonusPaynow { get; set; }
        public double? BonusVolumn { get; set; }
    }
    public class BpClassify
    {
        [Key] public int Id { get; set; }
        public int BpId { get; set; }
        public int RegionId { get; set; }
        public Region? Region { get; set; }
        public int AreaId { get; set; }
        public Area? Area { get; set; }
        public int IndustryId { get; set; }
        public Industry? Industry { get; set; }
        public List<Brand> Brands { get; set; } = [];
        [NotMapped] public List<int> BrandIds { get; set; } = new List<int>();
        public List<ItemType> ItemType { get; set; } = [];
        [NotMapped] public List<int> ItemTypeIds { get; set; } = new List<int>();
        public List<BPSize>? Sizes { get; set; }
        [NotMapped] public List<int> BpSizeIds { get; set; } = new List<int>();
    }

    public class CRD1
    {
        public int Id { get; set; }
        public int? LocationId { get; set; }
        [MaxLength(25)] public string? Type { get; set; } = "S";
        [MaxLength(254)] public string? LocationName { get; set; }
        public int? AreaId { get; set; }
        [MaxLength(254)] public string? AreaName { get; set; }
        [MaxLength(1000)] public string? Address { get; set; }
        [MaxLength(50)] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(50)] public string? VehiclePlate { get; set; }
        [MaxLength(50)] public string? CCCD { get; set; }
        [MaxLength(100)] public string? Person { get; set; }
        public string? Note { get; set; }
        public int BPId { get; set; }
        public string LocationType { get; set; } = "DM";
        public string? Default { get; set; }
        [NotMapped] public string? Status { get; set; }
        [JsonIgnore] public BP? BP { get; set; }
    }

    public class DebtSap
    {
        public string CardCode { get; set; }
        public int GroupCode { get; set; }
        public string? U_GTHM { get; set; }
        public string? U_NHBL { get; set; }
        public string? U_HTBL { get; set; }
        public string? U_BDTBL { get; set; }
        public string? U_LHM { get; set; }
        public int? U_THTT { get; set; }
        
    }


    public class CRD2
    {
        public int Id { get; set; }
        [MaxLength(25)] public string? Type { get; set; } = "ItemType";
        public int TypeId { get; set; }
        [MaxLength(50)] public string TypeCode { get; set; } = "";
        [MaxLength(254)] public string TypeName { get; set; } = "";
        public int BPId { get; set; }
        [NotMapped] public string? Status { get; set; }
        [JsonIgnore] public BP? BP { get; set; }
    }
    public class CRD3Dto
    {
        public int Id { get; set; }
        public int PaymentMethodID { get; set; }
        [MaxLength(50)] public string PaymentMethodCode { get; set; } = "";
        [MaxLength(254)] public string PaymentMethodName { get; set; } = "";
        public double? Balance { get; set; }
        public int? Times { get; set; }
    }
    public class CRD3
    {
        public int Id { get; set; }

        public int PaymentMethodID { get; set; }
        [MaxLength(50)] public string PaymentMethodCode { get; set; } = "";
        [MaxLength(254)] public string PaymentMethodName { get; set; } = "";
        [NotMapped]
        public double BalanceLimit { get; set; }
        public double? Balance { get; set; }
        public int? Times { get; set; }
        [NotMapped] public string BankGuarantee { get; set; } = "";
        [NotMapped] public string StartLetterOfGuarantee { get; set; } = "";
        [NotMapped] public string LetterOfGuarantee { get; set; } = "";
        
        [NotMapped] public int? PaymentTerm;
        
        [NotMapped]
        public List<CRD4> Invoices { get; set; } = new List<CRD4>();
        public int BPId { get; set; }
        [NotMapped] public string? Status { get; set; }
        [JsonIgnore] public BP? BP { get; set; }
    }

    public class CRD4Sum
    {
        public int? PaymentMethodID { get; set; }
        public double? AmountOverdue { get; set; }
    }

    /// <summary>
    /// Chi tiết công nợ theo invoice từ SAP.
    ///
    /// ⚠ KNOWN ISSUE: schema có 2 [Key] (Id và InvoiceNumber). EF model treat
    /// InvoiceNumber làm PK (vì có [Key] explicit), nhưng SQL DB actual có
    /// PK trên Id. Kèm theo: Payment.DocCode FK → InvoiceNumber (alternate key
    /// không unique — 1689/1690 rows).
    ///
    /// Vì vậy refactor schema này nguy hiểm — phải design lại Payment relationship
    /// trước. Tạm thời đã add unique index (BPId, InvoiceNumber) qua SQL script
    /// (xem docs/db/CRD4_unique_index.sql) để DB enforce dedup, KHÔNG sửa annotation.
    ///
    /// TODO khi có thời gian: redesign Payment.DocCode → CRD4.Id (FK tới surrogate
    /// key int) thay vì link qua InvoiceNumber string.
    /// </summary>
    public class CRD4
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key] public string InvoiceNumber { get; set; } = "";
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? PaymentMethodID { get; set; }
        [MaxLength(50)] public string? PaymentMethodCode { get; set; } = "";
        [MaxLength(254)] public string? PaymentMethodName { get; set; } = "";
        public double? InvoiceTotal { get; set; }
        public double? PaidAmount { get; set; }
        public double? AmountOverdue { get; set; }
        [NotMapped] public string TypeOfDebt { get; set; } = "C";
        public int BPId { get; set; }
        [NotMapped] public string? Status { get; set; }
        [NotMapped] public ICollection<Payment>? Payments { get; set; }
        [JsonIgnore] public BP? BP { get; set; }
    }

    public class CRD5
    {
        public int Id { get; set; }
        [MaxLength(50)] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(50)] public string? Phone1 { get; set; }

        [MaxLength(100)] public string? Person { get; set; }
        public string? Note { get; set; }
        public int BPId { get; set; }
        public string? Default { get; set; }
        [NotMapped] public string? Status { get; set; }
        [JsonIgnore] public BP? BP { get; set; }
    }

    public class CRD6
    {
        [Key] public int Id { get; set; }
        public string? FileName { get; set; }
        public Guid? FileId { get; set; }
        public int? BPId { get; set; }
        public string? Note { get; set; }
        public int? AuthorId { get; set; }
        public string? AuthorName => Author?.FullName;
        public AppUser? Author { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string? FileUrl { get; set; }
    }
    public class CustomerPoint
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public int DocId { get; set; }
        public int DocType { get; set; }
        public bool AddPoint { get; set; }
        public double TotalPointChange { get; set; }
        public ICollection<CustomerPointLine>? Details { get; set; }
    }
    public class CustomerPointLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public int FatherId { get; set; }
        [Newtonsoft.Json.JsonIgnore] public CustomerPoint? CustomerPoint { get; set; }
        public int? DocId { get; set; }
        public int? DocType { get; set; }

        public int? ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        [Required]
        public int PoitnSetupId { get; set; }

        [Required]
        public double PointChange { get; set; }

        public double PointBefore { get; set; }

        public double PointAfter { get; set; }

        [Required]
        public DateTime? DocDate { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }
    }
    //public class CustomerPointHistory
    //{
    //    [Key]
    //    public int Id { get; set; }

    //    [Required]
    //    public int CustomerId { get; set; }
    //    public int? DocId { get; set; }
    //    public int? DocType { get; set; }

    //    public int? ItemId { get; set; }
    //    public string ItemCode { get; set; }
    //    public string ItemName { get; set; }
    //    [Required]
    //    public int PoitnSetupId { get; set; }

    //    [Required]
    //    public double PointChange { get; set; } 

    //    public double PointBefore { get; set; }

    //    public double PointAfter { get; set; }

    //    [Required]
    //    public DateTime? DocDate { get; set; }

    //    [MaxLength(500)]
    //    public string Note { get; set; }
    //}
    public class CustomerPointCycle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int PoitnSetupId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        private double _earnedPoint;
        public double EarnedPoint
        {
            get => _earnedPoint;
            set
            {
                if (value < RemainingPoint)
                    throw new Exception("EarnedPoint không được nhỏ hơn RemainingPoint");
                _earnedPoint = value;
            }
        }

        private double _redeemedPoint;
        public double RedeemedPoint
        {
            get => _redeemedPoint;
            set
            {
                if (value < 0)
                    throw new Exception("RedeemedPoint không được nhỏ hơn 0");
                _redeemedPoint = value;
            }
        }

        private double _remainingPoint;
        public double RemainingPoint
        {
            get => _remainingPoint;
            set
            {
                if (value < 0)
                    throw new Exception("RemainingPoint không được nhỏ hơn 0");
                _remainingPoint = value;
            }
        }

        /// <summary>
        /// 0: Đang tích lũy, 1: Đã chốt, 2: Hết hạn
        /// </summary>
        public byte Status { get; set; }
    }

    public class CustomerPointCycleDTO
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        [Required]
        public int PoitnSetupId { get; set; }

        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public double EarnedPoint { get; set; }

        public double RedeemedPoint { get; set; }

        public double RemainingPoint { get; set; }

        /// <summary>
        /// 0: Đang tích lũy, 1: Đã chốt, 2: Hết hạn
        /// </summary>
        public byte Status { get; set; }
    }
}