using B1SLayer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BackEndAPI.Service.Sync.Health;

/// <summary>
/// Health check ping SAP B1 Service Layer. Trả Degraded nếu SAP không trả lời —
/// báo cho ops/load-balancer biết tích hợp SAP đang fail mà không xanh-đỏ toàn app.
/// </summary>
public class SapHealthCheck : IHealthCheck
{
    private readonly SLConnection _sl;
    private readonly ILogger<SapHealthCheck> _logger;

    public SapHealthCheck(SLConnection sl, ILogger<SapHealthCheck> logger)
    {
        _sl = sl;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            // Light-weight ping: chỉ login (B1SLayer cache session, login no-op nếu còn valid).
            await _sl.LoginAsync();

            return HealthCheckResult.Healthy("SAP B1 Service Layer reachable");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded("SAP B1 Service Layer timeout (5s)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SAP health check failed");
            return HealthCheckResult.Degraded($"SAP B1 Service Layer error: {ex.Message}");
        }
    }
}
