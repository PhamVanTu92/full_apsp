using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemGroups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Pipelines;
using System.Text.Json.Serialization;
using BackEndAPI.Models.Account;

namespace BackEndAPI.Models.Committed
{
    public class Committed
    {
        public int Id { get; set; }
        [MaxLength(25)] public string CommittedCode { get; set; }
        [MaxLength(100)] public string CommittedName { get; set; }
        [MaxLength(1000)] public string? CommittedDescription { get; set; }

        public int CardId { get; set; }
        [MaxLength(50)] public string? CardCode => BP?.CardCode;
        [MaxLength(254)] public string? CardName => BP?.CardName;
        [NotMapped][JsonIgnore] public BP? BP { get; set; }
        public DateTime? CommittedYear { get; set; }
        public int UserId { get; set; }
        public AppUser? Creator { get; set; }

        [MaxLength(25)] public string DocStatus { get; set; } = "P";
        public string? RejectReason { get; set; } = "";
        public List<CommittedLine> CommittedLine { get; set; } = new List<CommittedLine>();
        [NotMapped] public string? UserType { get; set; }
    }
    public class CommittedLine
    {
        public int Id { get; set; }

        public int FatherId { get; set; }
        [MaxLength(25)] public string CommittedType { get; set; }
        [NotMapped] public string Status { get; set; } = "";
        public List<CommittedLineSub> CommittedLineSub { get; set; } = [];

        public bool ValidateField()
        {
            bool isValid = true;
            double sumCommtied = 0;
            this.CommittedLineSub?.ForEach(p =>
            {
                switch (this.CommittedType)
                {
                    case "Q":
                        sumCommtied = p.Quarter1 ?? 0 + p.Quarter2 ?? 0 + p.Quarter3 ?? 0 + p.Quarter4 ?? 0;
                        break;
                    case "Y":
                        sumCommtied = p.Month1 ?? 0 + p.Month2 ?? 0 + p.Month3 ?? 0 + p.Month4 ??
                            0 + p.Month5 ?? 0 + p.Month6 ?? 0 + p.Month7 ?? 0 + p.Month8 ??
                            0 + p.Month9 ?? 0 + p.Month10 ?? 0 + p.Month11 ?? 0 + p.Month12 ?? 0;
                        break;
                }

                if (sumCommtied == 0 && p.Total == 0) isValid = false;
                if (p.Discount <= 0.0) isValid = false;
            });

            return isValid;
        }
    }
    public class CommittedLineSub
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int? IndustryId { get; set; }
        [NotMapped] public List<int> BrandIds { get; set; } = [];
        [NotMapped] public string? IndustryName => Industry?.Name;
        public int? BrandId { get; set; }

        [NotMapped] public List<int> ItemTypeIds { get; set; } = [];
        public List<ItemType> ItemTypes { get; set; } = [];
        public double? Quarter1 { get; set; }
        public double BonusQuarter1 { get; set; } = 0;
        public double TotalQuarter1 { get; set; } = 0;
        public double? Quarter2 { get; set; }
        public double BonusQuarter2 { get; set; }
        public double TotalQuarter2 { get; set; } = 0;
        public double? Quarter3 { get; set; }
        public double BonusQuarter3 { get; set; } = 0;
        public double TotalQuarter3 { get; set; } = 0;
        public double? Quarter4 { get; set; }
        public double BonusQuarter4 { get; set; } = 0;
        public double TotalQuarter4 { get; set; } = 0;

        public double? Package { get; set; }
        public double? Month1 { get; set; }
        public double BonusMonth1 { get; set; } = 0;
        public double TotalMonth1 { get; set; } = 0;
        public double? Month2 { get; set; }
        public double BonusMonth2 { get; set; } = 0;
        public double TotalMonth2 { get; set; } = 0;
        public double? Month3 { get; set; }
        public double BonusMonth3 { get; set; } = 0;
        public double TotalMonth3 { get; set; } = 0;
        public double? Month4 { get; set; }
        public double BonusMonth4 { get; set; } = 0;
        public double TotalMonth4 { get; set; } = 0;
        public double? Month5 { get; set; }
        public double BonusMonth5 { get; set; } = 0;
        public double TotalMonth5 { get; set; } = 0;
        public double? Month6 { get; set; }
        public double BonusMonth6 { get; set; } = 0;
        public double TotalMonth6 { get; set; } = 0;
        public double? Month7 { get; set; }
        public double BonusMonth7 { get; set; } = 0;
        public double TotalMonth7 { get; set; } = 0;
        public double? Month8 { get; set; }
        public double BonusMonth8 { get; set; } = 0;
        public double TotalMonth8 { get; set; } = 0;
        public double? Month9 { get; set; }
        public double BonusMonth9 { get; set; } = 0;
        public double TotalMonth9 { get; set; } = 0;
        public double? Month10 { get; set; }
        public double BonusMonth10 { get; set; } = 0;
        public double TotalMonth10 { get; set; } = 0;
        public double? Month11 { get; set; }
        public double BonusMonth11 { get; set; } = 0;
        public double TotalMonth11 { get; set; } = 0;
        public double? Month12 { get; set; }
        public double BonusMonth12 { get; set; } = 0;
        public double TotalMonth12 { get; set; } = 0;

        public double? Total { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "The quarter must be greater than 0")]
        public double? Discount { get; set; }

        public double? DiscountYear { get; set; }
        public bool? IsCvYear { get; set; }
        public bool? IsConvert { get; set; } = false;

        public double? SixMonthDiscount { get; set; }
        public bool IsCvSixMonth { get; set; } = false;
        public double? SixMonthBonus { get; set; }

        public double? ThreeMonthDiscount { get; set; }
        public double? ThreeMonthBonus { get; set; }
        public bool IsCvThreeMonth { get; set; } = false;

        public double? DiscountMonth { get; set; }

        public bool? IsCvMonth { get; set; } = false;

        public double? NineMonthDiscount { get; set; }
        public double? NineMonthBonus { get; set; }
        public double? YearBonus { get; set; }

        public bool? IsCvNineMonth { get; set; } = false;
        public double CurrentVolumn { get; set; } = 0;
        public double? Total12M { get; set; } = 0;
        [NotMapped] public double AfterVolumn { get; set; } = 0;
        [NotMapped] public bool IsAchieved { get; set; } = false;
        [NotMapped] public double BonusPercentage { get; set; } = 0;
        [NotMapped] public double TotalBonus { get; set; } = 0;
        [NotMapped] public Industry? Industry { get; set; }
        [NotMapped] public string Status { get; set; } = "";
        public List<Brand> Brand { get; set; } = [];
        public ICollection<CommittedLineSubSub>? CommittedLineSubSub { get; set; }
    }
    public class CommittedLineSubSub
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public double OutPut { get; set; }

        public double BonusTotal { get; set; } = 0;
        public double Total { get; set; } = 0;
        public double Discount { get; set; }
        public bool IsConvert { get; set; }
        [NotMapped] public string Status { get; set; } = "";
    }
    public class CommittedTracking
    {
        public int Id { get; set; }
        public int CommittedId { get; set; }
        public ICollection<CommittedTrackingLine> CommittedTrackingLine { get; set; }
    }
    public class CommittedTrackingLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int CommittedLineSubId { get; set; }
        public int DocId { get; set; }
        public bool IsCommitted { get; set; } = true;
        [NotMapped]
        public bool? IsCheck { get; set; } = true;
        public string CommittedType { get; set; }
        public double Volume { get; set; }
        public DateTime OrderDate { get; set; }
        public double? BonusAddApplied { get; set; }
        public double BonusApplied { get; set; }
        public CommittedLineSub? CommittedLineSub { get; set; }
    }
}