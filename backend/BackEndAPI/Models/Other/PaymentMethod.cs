using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Other
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; }
        [MaxLength(254)]
        public string PaymentMethodName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
    }
    public class PaymentMethodCreate
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; }
        [MaxLength(254)]
        public string PaymentMethodName { get; set; }
        [MaxLength(10)]
        public string PaymentMethodType { get; set; }
        [MaxLength(10)]
        public string Type { get; set; }
        [MaxLength(50)]
        public string? Creator { get; set; }
    }
}
