using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Account;

/// <summary>
/// Refresh token để gia hạn access token mà không cần user nhập credential lại.
/// Token raw (gốc) chỉ trả 1 lần khi tạo — DB chỉ lưu hash để chống leak.
///
/// Rotation: mỗi lần dùng refresh token, tạo token mới + revoke token cũ.
/// Reuse phát hiện được qua check IsRevoked → có thể là dấu hiệu token bị steal.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>SHA-256 hash của token (base64). Raw token chỉ tồn tại trong response.</summary>
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash của token thay thế khi rotate — null nếu chưa bị rotate.</summary>
    [MaxLength(128)]
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>Lý do revoke: "Logout", "Rotated", "Compromised", "Expired"...</summary>
    [MaxLength(50)]
    public string? RevocationReason { get; set; }

    /// <summary>IP của request tạo refresh token (audit).</summary>
    [MaxLength(64)]
    public string? CreatedByIp { get; set; }

    /// <summary>IP của request revoke (audit).</summary>
    [MaxLength(64)]
    public string? RevokedByIp { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
