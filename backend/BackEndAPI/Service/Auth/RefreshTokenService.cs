using BackEndAPI.Data;
using BackEndAPI.Models.Account;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BackEndAPI.Service.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    /// <summary>Refresh token sống 7 ngày — đủ cho user dùng bình thường, không quá lâu nếu bị steal.</summary>
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(7);

    /// <summary>Độ dài raw token (bytes trước base64). 64 bytes = 512 bit entropy → an toàn.</summary>
    private const int RawTokenBytes = 64;

    private readonly AppDbContext _db;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(AppDbContext db, ILogger<RefreshTokenService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(string token, DateTime expiresAt)> CreateAsync(int userId, string? ip, CancellationToken ct)
    {
        var rawToken = GenerateRawToken();
        var hash = Hash(rawToken);
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(TokenLifetime);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            CreatedByIp = ip
        });
        await _db.SaveChangesAsync(ct);

        return (rawToken, expiresAt);
    }

    public async Task<(int userId, string newToken, DateTime expiresAt)> RotateAsync(string rawToken, string? ip, CancellationToken ct)
    {
        var hash = Hash(rawToken);
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (existing == null)
        {
            _logger.LogWarning("Refresh token rotation: unknown token hash from IP {Ip}", ip);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (existing.IsExpired)
        {
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        if (existing.IsRevoked)
        {
            // Reuse phát hiện — token đã revoke nhưng vẫn được gửi lại.
            // Nhiều khả năng token bị steal: revoke toàn bộ token của user để ép logout.
            _logger.LogWarning(
                "Refresh token REUSE detected for user {UserId} from IP {Ip} — revoking all user tokens",
                existing.UserId, ip);
            await RevokeAllForUserAsync(existing.UserId, ip, "Compromised - reuse detected", ct);
            throw new UnauthorizedAccessException("Refresh token compromised");
        }

        // Tạo token mới
        var newRaw = GenerateRawToken();
        var newHash = Hash(newRaw);
        var now = DateTime.UtcNow;
        var newExpiresAt = now.Add(TokenLifetime);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = newHash,
            CreatedAt = now,
            ExpiresAt = newExpiresAt,
            CreatedByIp = ip
        });

        // Revoke token cũ
        existing.RevokedAt = now;
        existing.RevokedByIp = ip;
        existing.RevocationReason = "Rotated";
        existing.ReplacedByTokenHash = newHash;

        await _db.SaveChangesAsync(ct);

        return (existing.UserId, newRaw, newExpiresAt);
    }

    public async Task RevokeAsync(string rawToken, string? ip, string reason, CancellationToken ct)
    {
        var hash = Hash(rawToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token == null || token.IsRevoked) return;

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ip;
        token.RevocationReason = reason;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(int userId, string? ip, string reason, CancellationToken ct)
    {
        var active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var t in active)
        {
            t.RevokedAt = now;
            t.RevokedByIp = ip;
            t.RevocationReason = reason;
        }
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}: {Reason}",
            active.Count, userId, reason);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static string GenerateRawToken()
    {
        var bytes = new byte[RawTokenBytes];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>SHA-256 hash → base64. Không cần salt vì raw token đã đủ entropy.</summary>
    private static string Hash(string raw)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(raw);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
