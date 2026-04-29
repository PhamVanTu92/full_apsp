using BackEndAPI.Models.Account;
using BackEndAPI.Models.Unit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Fee
{
    public class Fee
    {
        public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; }
        [MaxLength(250)] public string Description { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        [NotMapped] public AppUser? appUser { get; set; }
        public bool Status { get; set; }
        public ICollection<FeeLine>? FeeLine { get; set; }
    }

    public class FeeParam
    {
        public string? Name { get; set; }
        [MaxLength(250)] public string? Description { get; set; }
        public bool Status { get; set; }
    }

    public class FeeLine
    {
        public int Id { get; set; }
        public int? FeeId { get; set; }

        public int UgpId { get; set; }
        public int FeeLevelId { get; set; }
        public double? FeePrice { get; set; }
        public double? FeeWAT { get; set; }
        [NotMapped] public string? Status { get; set; }
        public OUGP? OUGP { get; set; }
        public FeeLevel? FeeLevel { get; set; }
        [JsonIgnore] public Fee? Fee { get; set; }
    }

    public class FeeLevel
    {
        public int Id { get; set; }
        [MaxLength(100)] public string Name { get; set; }
        public int FromDays { get; set; }
        public int? ToDays { get; set; }
        [NotMapped] public string? Status { get; set; }
    }


    public class FeePeriod
    {
        public int Id { get; set; }
        public int Period { get; set; }
        public int Year { get; set; }
        public ICollection<FeePeriodLine>? FeePeriodLine { get; set; }
    }

    public class FeePeriodLine
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        [MaxLength(50)] public string ItemCode { get; set; }
        [MaxLength(254)] public string ItemName { get; set; }
        [MaxLength(50)] public string CardCode { get; set; }
        [MaxLength(254)] public string CardName { get; set; }

        [MaxLength(50)] public string BatchNum { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime IssueDate { get; set; }
        public int Day { get; set; }
        [MaxLength(50)] public string UgpCode { get; set; }
        [MaxLength(50)] public string UgpName { get; set; }

        public double Quantity { get; set; }
        [JsonIgnore] public FeePeriod? FeePeriod { get; set; }
    }

    public class FeebyCustomer
    {
        public int Id { get; set; }
        [MaxLength(50)] public string CardCode { get; set; }
        [MaxLength(254)] public string CardName { get; set; }
        public int Year { get; set; }
        public int Period { get; set; }
        [MaxLength(254)] public string FeebyCustomerCode { get; set; }
        [MaxLength(254)] public string FeebyCustomerName { get; set; }

        public double Total { get; set; }
        public double Vat { get; set; }
        [MaxLength(25)] public string Status { get; set; } = "NS";
        [MaxLength(25)] public string ConfirmStatus { get; set; } = "NC";

        [MaxLength(25)] public string PayStatus { get; set; } = "NP";
        public string? Note { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? SendedmDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? PayedDate { get; set; }
        public ICollection<FeebyCustomerLine>? FeebyCustomerLine { get; set; }
    }

    public class FeebyCustomerLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        [MaxLength(50)] public string ItemCode { get; set; }
        [MaxLength(254)] public string ItemName { get; set; }
        [MaxLength(50)] public string BatchNum { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime IssueDate { get; set; }
        public int Day { get; set; }
        [MaxLength(50)] public string UgpCode { get; set; }
        [MaxLength(50)] public string UgpName { get; set; }

        public double Quantity { get; set; }
        public int DayToFee { get; set; }

        public int FeeLevelId { get; set; }
        public string FeeLevelName { get; set; }
        public double Price { get; set; }
        public double LineTotal { get; set; }
        public double LineVAT { get; set; }

        [JsonIgnore] public FeebyCustomer? FeebyCustomer { get; set; }
    }
}