using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models;

public class SystemLog
{
    [Key] public int Id { get; set; } // Khóa chính

    public DateTime Timestamp { get; set; } = DateTime.Now; // Thời điểm log

    public string? ObjType { get; init; }
    public int? ObjId { get; init; }

    public string Level { get; set; } = "INFO"; // Mức độ log: INFO, WARN, ERROR, DEBUG, etc.

    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty; // Nội dung log

    public int ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActorUserName { get; set; } = string.Empty;

    public string? Logger { get; set; } // Tên class hoặc service tạo log

    public string? Exception { get; set; } // Nếu có lỗi thì lưu chuỗi lỗi ở đây

    // public string? UserName { get; set; } // Người thực hiện (nếu có)

    public string? IpAddress { get; set; } // Địa chỉ IP

    public string? Url { get; set; } // URL (nếu là web)

    /// <summary>
    /// Thiết bị
    /// </summary>
    public string? Device { get; init; }

    public string? HttpMethod { get; set; } // GET, POST, PUT,...

    public string? RequestBody { get; set; } // Nội dung request (nếu cần)

    public string? ResponseBody { get; set; } // Nội dung response (nếu cần)
}

public class PolicyOrderLog
{
    [Key] public int Id { get; init; }

    /// <summary>
    /// Mã khách hàng
    /// </summary>
    public string CardCode { get; init; } = string.Empty;

    /// <summary>
    /// Tên khách hàng
    /// </summary>
    public string CardName { get; init; } = string.Empty;

    /// <summary>
    /// Mã đơn hàng
    /// </summary>
    public string OrderCode { get; init; } = string.Empty;

    public string PolicyHash { get; init; } = string.Empty;

    /// <summary>
    /// Phiên bản chính sách
    /// </summary>
    public string PolicyVersion { get; init; } = string.Empty;

    /// <summary>
    /// Thời điểm đồng ý
    /// </summary>
    public DateTime AgreeAt { get; init; }

    public int PolicyId { get; init; }

    /// <summary>
    /// Địa chỉ IP
    /// </summary>
    public string IpAddress { get; init; } = string.Empty;

    /// <summary>
    /// Thiết bị
    /// </summary>
    public string Device { get; init; } = string.Empty;

    public int OrderId { get; init; }
}