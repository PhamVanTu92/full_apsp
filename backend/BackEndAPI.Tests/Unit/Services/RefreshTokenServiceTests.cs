using BackEndAPI.Service.Auth;
using BackEndAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Test cho RefreshTokenService — security-critical, phải verify:
///   • Token mới có entropy cao (không predictable)
///   • Rotation: token cũ revoked + token mới issued
///   • Reuse detection: dùng lại token đã revoked → revoke all + throw
///   • Expire / revoke handling đúng
/// </summary>
public class RefreshTokenServiceTests
{
    private static RefreshTokenService Create(out Data.AppDbContext db)
    {
        db = TestDbContextFactory.Create();
        return new RefreshTokenService(db, NullLogger<RefreshTokenService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ReturnsRawTokenAndPersistsHash()
    {
        var sut = Create(out var db);

        var (token, expiresAt) = await sut.CreateAsync(userId: 42, ip: "127.0.0.1", default);

        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(80); // 64 bytes base64 ~ 88 chars
        expiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(6));
        expiresAt.Should().BeBefore(DateTime.UtcNow.AddDays(8));

        db.RefreshTokens.Should().HaveCount(1);
        var stored = db.RefreshTokens.First();
        stored.UserId.Should().Be(42);
        stored.TokenHash.Should().NotBe(token, "DB chỉ lưu hash, không lưu raw");
        stored.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_TwoCallsProduceDifferentTokens()
    {
        var sut = Create(out _);

        var (t1, _) = await sut.CreateAsync(1, null, default);
        var (t2, _) = await sut.CreateAsync(1, null, default);

        t1.Should().NotBe(t2);
    }

    [Fact]
    public async Task RotateAsync_RevokesOldAndIssuesNew()
    {
        var sut = Create(out var db);
        var (oldToken, _) = await sut.CreateAsync(7, "1.1.1.1", default);

        var (userId, newToken, expiresAt) = await sut.RotateAsync(oldToken, "2.2.2.2", default);

        userId.Should().Be(7);
        newToken.Should().NotBe(oldToken);
        expiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(6));

        db.RefreshTokens.Should().HaveCount(2);
        var oldRecord = db.RefreshTokens.OrderBy(t => t.Id).First();
        var newRecord = db.RefreshTokens.OrderBy(t => t.Id).Last();

        oldRecord.IsRevoked.Should().BeTrue();
        oldRecord.RevocationReason.Should().Be("Rotated");
        oldRecord.RevokedByIp.Should().Be("2.2.2.2");
        oldRecord.ReplacedByTokenHash.Should().Be(newRecord.TokenHash);

        newRecord.IsActive.Should().BeTrue();
        newRecord.UserId.Should().Be(7);
    }

    [Fact]
    public async Task RotateAsync_WithUnknownToken_Throws()
    {
        var sut = Create(out _);

        var act = () => sut.RotateAsync("not-a-real-token", null, default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RotateAsync_ReuseRevokedToken_TriggersRevokeAll()
    {
        var sut = Create(out var db);
        var (token1, _) = await sut.CreateAsync(99, null, default);
        var (_, _, _) = await sut.RotateAsync(token1, null, default); // token1 đã revoked

        // Tạo thêm 1 token nữa cho user 99 để verify revoke-all
        var (token2, _) = await sut.CreateAsync(99, null, default);

        // Reuse token1 (đã revoked) → phải trigger revoke all
        var act = () => sut.RotateAsync(token1, null, default);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*compromised*");

        // Sau khi compromise detected: token2 (đang active) cũng bị revoke
        db.ChangeTracker.Clear();
        var allTokens = db.RefreshTokens.Where(t => t.UserId == 99).ToList();
        allTokens.Should().OnlyContain(t => t.IsRevoked, "tất cả token user 99 phải bị revoke");
    }

    [Fact]
    public async Task RevokeAsync_MarksTokenRevoked()
    {
        var sut = Create(out var db);
        var (token, _) = await sut.CreateAsync(5, null, default);

        await sut.RevokeAsync(token, "3.3.3.3", "Logout", default);

        db.ChangeTracker.Clear();
        var stored = db.RefreshTokens.First();
        stored.IsRevoked.Should().BeTrue();
        stored.RevocationReason.Should().Be("Logout");
        stored.RevokedByIp.Should().Be("3.3.3.3");
    }

    [Fact]
    public async Task RevokeAllForUserAsync_OnlyRevokesActiveTokensForUser()
    {
        var sut = Create(out var db);
        await sut.CreateAsync(1, null, default);
        await sut.CreateAsync(1, null, default);
        await sut.CreateAsync(2, null, default); // khác user, không bị revoke

        await sut.RevokeAllForUserAsync(1, null, "Logout all", default);

        db.ChangeTracker.Clear();
        var user1Tokens = db.RefreshTokens.Where(t => t.UserId == 1).ToList();
        var user2Tokens = db.RefreshTokens.Where(t => t.UserId == 2).ToList();

        user1Tokens.Should().OnlyContain(t => t.IsRevoked);
        user2Tokens.Should().OnlyContain(t => !t.IsRevoked);
    }
}
