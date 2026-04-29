using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Account;
using Newtonsoft.Json;

namespace BackEndAPI.Models.SaleForecastModel;

public class SaleForecast
{
    [Key] public int Id { get; set; } // Khóa chính cho kế hoạch đặt hàng
    [MaxLength(255)] public string? PlanName { get; set; }
    [MaxLength(255)] public string? PlanCode { get; set; }
    public int CustomerId { get; init; }
    [NotMapped] public string? CustomerName => Bp?.CardName;
    [NotMapped] public string? CustomerCode => Bp?.CardCode;

    public DateTime PlanDate { get; set; } // Ngày lập kế hoạch

    // public DateTime ExpectedDeliveryDate { get; set; } // Ngày giao hàng dự kiến
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    [MaxLength(15)] public string? Status { get; set; }   // Trạng thái kế hoạch
    [MaxLength(1000)] public string? Notes { get; init; } // Ghi chú bổ sung
    [MaxLength(1)] public string? PeriodType { get; init; }
    public string? HashData { get; set; }

    public int UserId { get; set; }
    [NotMapped] public AppUser? Author { get; init; }

    [NotMapped]
    [System.Text.Json.Serialization.JsonIgnore]
    public BP? Bp { get; init; }

    // Danh sách sản phẩm trong kế hoạch
    public List<SaleForecastItem> SaleForecastItems { get; init; } = new List<SaleForecastItem>();

    [Obsolete("Obsolete")]
    public void BuildHashData()
    {
        var serialized = System.Text.Json.JsonSerializer.Serialize(this);

        using var sha256    = SHA256.Create();
        var       hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(serialized));
        HashData = Convert.ToBase64String(hashBytes);
    }
}