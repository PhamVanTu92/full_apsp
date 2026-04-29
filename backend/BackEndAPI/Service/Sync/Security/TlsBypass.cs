using System.Net.Security;

namespace BackEndAPI.Service.Sync.Security;

/// <summary>
/// TLS cert bypass dùng cho SAP B1 self-signed cert.
/// Chỉ kích hoạt khi:
///   • ASPNETCORE_ENVIRONMENT == "Development", HOẶC
///   • Env var ALLOW_SELF_SIGNED_TLS == "true" (escape hatch cho staging với self-signed cert)
///
/// Production: trả null → caller nhận behavior mặc định của .NET (validate cert nghiêm).
/// Khi đó nếu SAP dùng self-signed cert, request sẽ fail với
/// AuthenticationException "RemoteCertificateNameMismatch" — đó là intent đúng.
/// </summary>
public static class TlsBypass
{
    private static readonly bool _enabled =
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("ALLOW_SELF_SIGNED_TLS"), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Trả về callback "accept any cert" nếu được phép (Development),
    /// null nếu không (Production) — assign null vào event là no-op.
    /// </summary>
    public static RemoteCertificateValidationCallback? AcceptSelfSignedInDev()
        => _enabled ? (_, _, _, _) => true : null;

    /// <summary>True nếu TLS bypass đang được kích hoạt — dùng để log warning lúc startup.</summary>
    public static bool IsEnabled => _enabled;
}
