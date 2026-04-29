using Quartz;
using System.Diagnostics;

namespace BackEndAPI.Service.Sync;

/// <summary>
/// Base class cho các Quartz sync job với logging có scope, đo elapsed time,
/// xử lý exception nhất quán và tôn trọng CancellationToken.
///
/// Cách dùng:
///     public class SyncFooJob : SyncJobBase
///     {
///         public SyncFooJob(ILogger&lt;SyncFooJob&gt; logger, IFooService fooService) : base(logger)
///         { _fooService = fooService; }
///
///         protected override Task RunAsync(CancellationToken ct) => _fooService.SyncFoo(ct);
///     }
/// </summary>
[DisallowConcurrentExecution]
public abstract class SyncJobBase : IJob
{
    private readonly ILogger _logger;

    protected SyncJobBase(ILogger logger) => _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        var jobName = GetType().Name;
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobName"] = jobName,
            ["FireInstanceId"] = context.FireInstanceId,
            ["ScheduledFireTime"] = context.ScheduledFireTimeUtc?.ToString("o") ?? "n/a"
        });

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("[{JobName}] start", jobName);

        try
        {
            await RunAsync(context.CancellationToken);
            sw.Stop();
            _logger.LogInformation("[{JobName}] success in {ElapsedMs}ms", jobName, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            _logger.LogWarning("[{JobName}] cancelled after {ElapsedMs}ms", jobName, sw.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[{JobName}] failed after {ElapsedMs}ms", jobName, sw.ElapsedMilliseconds);

            // Đẩy lên Quartz để JobListener / monitoring biết. refireImmediately=false để không retry vô hạn.
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    /// <summary>Logic sync thực sự — implement ở subclass.</summary>
    protected abstract Task RunAsync(CancellationToken cancellationToken);
}
