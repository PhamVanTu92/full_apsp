using BackEndAPI.Data;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace BackEndAPI.Tests.Performance;

/// <summary>
/// Shared plumbing for the perf benchmarks: read connection string from
/// <c>APSP_BENCH_CONN</c>, build an <see cref="AppDbContext"/> against
/// real SQL Server, and provide a small Stopwatch-based timing helper.
/// </summary>
public abstract class BenchmarkBase : IAsyncLifetime
{
    private const string EnvVarName = "APSP_BENCH_CONN";

    protected readonly ITestOutputHelper Output;
    protected string? ConnectionString { get; private set; }

    protected BenchmarkBase(ITestOutputHelper output)
    {
        Output = output;
    }

    public Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable(EnvVarName);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Build a fresh DbContext per call — benchmarks should use distinct
    /// contexts so that EF first-level cache from a prior measurement
    /// does not bleed into the next.
    /// </summary>
    protected AppDbContext NewContext()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException(
                $"Set the {EnvVarName} environment variable to a SQL Server connection string before running benchmarks.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableDetailedErrors()
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Run <paramref name="action"/> <paramref name="iterations"/> + 1 times
    /// (extra run is the discarded warm-up), return the median elapsed
    /// time of the kept iterations.
    /// </summary>
    protected async Task<TimeSpan> MeasureMedianAsync(string label, int iterations, Func<Task> action)
    {
        // Warm-up — JIT, EF model build, SQL plan cache.
        await action();

        var samples = new List<TimeSpan>(iterations);
        for (var i = 0; i < iterations; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await action();
            sw.Stop();
            samples.Add(sw.Elapsed);
        }

        samples.Sort();
        var median = samples[samples.Count / 2];
        Output.WriteLine($"  {label,-48} median={median.TotalMilliseconds,8:F2} ms  samples=[{string.Join(", ", samples.Select(s => s.TotalMilliseconds.ToString("F1")))}]");
        return median;
    }

    protected void SkipIfUnconfigured()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            // xUnit has no native runtime-skip; emit a clear marker and assert true so
            // the test passes vacuously rather than misreporting a perf regression.
            Output.WriteLine($"[skipped] {EnvVarName} not set — benchmark cannot run without a real SQL Server.");
            throw new SkipException($"{EnvVarName} not set");
        }
    }
}

/// <summary>
/// Minimal "skipped at runtime" exception. xUnit 2.x does not have a
/// built-in dynamic skip; we throw a typed exception that the test
/// translates into a passing-but-skipped result via try/catch.
/// </summary>
internal sealed class SkipException : Exception
{
    public SkipException(string message) : base(message) { }
}
