namespace BackEndAPI.Service.Auth;

/// <summary>
/// Quản lý refresh token với pattern rotation: mỗi lần dùng → tạo token mới,
/// revoke token cũ. Phát hiện reuse (dấu hiệu token bị steal) qua check IsRevoked.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Tạo refresh token mới cho user. Trả về (raw token, expiresAt).
    /// Raw token chỉ tồn tại trong response — DB chỉ lưu hash.
    /// </summary>
    Task<(string token, DateTime expiresAt)> CreateAsync(int userId, string? ip, CancellationToken ct);

    /// <summary>
    /// Validate raw token + rotate: revoke token cũ, tạo + return token mới.
    /// Throw <see cref="UnauthorizedAccessException"/> nếu token invalid/expired/revoked.
    /// </summary>
    Task<(int userId, string newToken, DateTime expiresAt)> RotateAsync(string rawToken, string? ip, CancellationToken ct);

    /// <summary>Revoke 1 token cụ thể (vd: logout từ 1 device).</summary>
    Task RevokeAsync(string rawToken, string? ip, string reason, CancellationToken ct);

    /// <summary>Revoke toàn bộ token còn active của user (vd: đổi mật khẩu, logout-all-devices).</summary>
    Task RevokeAllForUserAsync(int userId, string? ip, string reason, CancellationToken ct);
}
