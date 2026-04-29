using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace BackEndAPI.Tests.Performance;

/// <summary>
/// Benchmark for BACKEND_REVIEW H-6
/// (Approval.CreateApprovalAsync — was loading every active WTM2 row,
/// then filtering in memory). The fix pushes the actor-membership
/// predicate into SQL via Where(...).FirstOrDefaultAsync().
/// </summary>
public class ApprovalLookupBench : BenchmarkBase
{
    public ApprovalLookupBench(ITestOutputHelper output) : base(output) { }

    private const int Iterations = 5;
    private const double MinSpeedupRatio = 2.0;

    [Fact(Skip = "Perf benchmark — set APSP_BENCH_CONN env var and remove Skip to run.")]
    public async Task Sql_filter_should_outperform_in_memory_filter()
    {
        try { SkipIfUnconfigured(); }
        catch (SkipException) { return; }

        // Pick a real ActorId from the user table that actually appears in some
        // OWTM.RUsers — gives the AFTER query at least one match to seek to.
        await using var probe = NewContext();

        var candidate = await probe.OWTM
            .AsNoTracking()
            .Where(o => o.Active)
            .SelectMany(o => o.RUsers)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (candidate == 0)
        {
            Output.WriteLine("[skipped] No RUsers found on any active OWTM — seed approval template data first.");
            return;
        }

        var wtm2RowCount = await probe.WTM2.AsNoTracking()
            .Where(w => w.OWTM.Active && w.Sort == 1)
            .CountAsync();

        Output.WriteLine($"WTM2 active+sort=1 rows: {wtm2RowCount:N0}; using ActorId={candidate}");

        if (wtm2RowCount < 100)
        {
            Output.WriteLine($"[warning] Only {wtm2RowCount} candidate rows — gap will be small.");
        }

        // BEFORE — load all active WTM2, FirstOrDefault in memory.
        var beforeMedian = await MeasureMedianAsync("BEFORE  WTM2.ToList() + FirstOrDefault", Iterations, async () =>
        {
            await using var ctx = NewContext();
            var apts = await ctx.WTM2
                .Include(a => a.OWST)
                .ThenInclude(b => b!.WST1)
                .Include(d => d.OWTM)
                .ThenInclude(d => d.RUsers)
                .Where(a => a.OWTM.Active)
                .Where(c => c.Sort == 1)
                .ToListAsync();

            _ = apts.FirstOrDefault(e =>
                e.OWTM.RUsers.Any(p => p.Id == candidate) && e.OWTM.RUsers.Count != 0);
        });

        // AFTER — push the membership predicate to SQL.
        var afterMedian = await MeasureMedianAsync("AFTER   AsNoTracking + Where + FirstOrDefaultAsync", Iterations, async () =>
        {
            await using var ctx = NewContext();
            _ = await ctx.WTM2.AsNoTracking()
                .Include(a => a.OWST)
                .ThenInclude(b => b!.WST1)
                .Include(d => d.OWTM)
                .ThenInclude(d => d.RUsers)
                .Where(a => a.OWTM.Active && a.Sort == 1)
                .Where(e => e.OWTM.RUsers.Any(p => p.Id == candidate))
                .FirstOrDefaultAsync();
        });

        var ratio = beforeMedian.TotalMilliseconds / Math.Max(afterMedian.TotalMilliseconds, 0.01);
        Output.WriteLine($"Speedup: {ratio:F1}x (BEFORE / AFTER, higher is better)");

        afterMedian.Should().BeLessThanOrEqualTo(beforeMedian,
            "AFTER returns at most one row across the network; BEFORE returns all matching candidates");

        if (wtm2RowCount >= 500)
        {
            ratio.Should().BeGreaterThan(MinSpeedupRatio,
                $"with {wtm2RowCount:N0} candidate rows the SQL-side filter should be at least {MinSpeedupRatio}x faster");
        }
    }
}
