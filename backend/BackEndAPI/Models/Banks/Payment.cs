using BackEndAPI.Models.Promotion;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndAPI.Models.BusinessPartners;

namespace BackEndAPI.Models.Banks
{
    public class Payment
    {
      
        public int Id { get; set; }
        [MaxLength(50)]
        public string? PaymentCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentMethodId { get; set; }
        [MaxLength(50)] public string PaymentMethodCode { get; set; } = "-";
        [MaxLength(254)]

        public string PaymentMethodName { get; set; } = "-";
        public int BranchId { get; set; }
        public double? TotalAmount { get; set; }
        public double PaymentAmount { get; set; }
        [NotMapped]
        public double RemainingAmount => TotalAmount ?? 0 - PaymentAmount;

        [MaxLength(10)] public string PartnerType { get; set; } = "C";
        public int? PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerContactNo { get; set; }
        [MaxLength(254)] public string Remarks { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public int DocId { get; set; }
        
        public string DocCode {  get; set; }
        [MaxLength(50)]
        public int? DocType { get; set; }
        [NotMapped]
        [JsonIgnore]
        public CRD4? Crd4 { get; set; }
        public ICollection<PaymentLine> PaymentLine { get; set; }
        public string Status { get; set; } = "P";

        public bool IsChecked { get; set; } = false;
        [NotMapped]
        public string RedirectLink { get; set; } = "";
    }

    public class PaymentDto
    {
        public int DocId { get; set; }
        public double PaymentAmount { get; set; }
        public int PaymentMethodId { get; set; }
    }
    public class PaymentLine
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string Method { get; set; }
        public string MethodStr { get; set; }
        public double Amount { get; set; }
        public int AccountId { get; set; }
        public string UsePoint { get; set; }
        [JsonIgnore]
        public Payment? Payment { get; set; }
    }
}
