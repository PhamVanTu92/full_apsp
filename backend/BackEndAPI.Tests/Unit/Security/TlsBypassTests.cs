using BackEndAPI.Service.Sync.Security;
using FluentAssertions;
using Xunit;

namespace BackEndAPI.Tests.Unit.Security;

/// <summary>
/// Test rằng TLS bypass chỉ kích hoạt trong Development hoặc với env var explicit.
/// Class quan trọng về security — phải đảm bảo Production không bypass cert.
///
/// Lưu ý: TlsBypass đọc env var 1 lần ở static constructor, không thể test
/// "đổi env var rồi gọi lại". Nhưng có thể verify behavior với env hiện tại.
/// </summary>
public class TlsBypassTests
{
    [Fact]
    public void IsEnabled_ReflectsCurrentEnvironment()
    {
        // Trong test runner, ASPNETCORE_ENVIRONMENT chưa được set ⇒ Production-like
        // (trừ khi developer run với env Development).
        // Verify rằng giá trị consistent với env var thực tế.
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var explicitFlag = Environment.GetEnvironmentVariable("ALLOW_SELF_SIGNED_TLS");

        var expectedEnabled =
            string.Equals(aspnetEnv, "Development", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(explicitFlag, "true", StringComparison.OrdinalIgnoreCase);

        TlsBypass.IsEnabled.Should().Be(expectedEnabled);
    }

    [Fact]
    public void AcceptSelfSignedInDev_WhenEnabled_ReturnsCallbackThatAcceptsAllCerts()
    {
        if (!TlsBypass.IsEnabled)
        {
            // Trong test này skip nếu env không cho bypass
            return;
        }

        var callback = TlsBypass.AcceptSelfSignedInDev();

        callback.Should().NotBeNull();
        // Gọi với args giả — tất cả phải return true
        var result = callback!.Invoke(sender: null!, certificate: null, chain: null,
            sslPolicyErrors: System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors);
        result.Should().BeTrue();
    }

    [Fact]
    public void AcceptSelfSignedInDev_WhenDisabled_ReturnsNull()
    {
        if (TlsBypass.IsEnabled)
        {
            // Skip — không thể verify nếu đang enabled
            return;
        }

        var callback = TlsBypass.AcceptSelfSignedInDev();

        callback.Should().BeNull("production phải dùng default TLS validation");
    }
}
