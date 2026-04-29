using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace BackEndAPI.Tests.Performance;

/// <summary>
/// Benchmark for BACKEND_REVIEW H-3 (DocumentService.SyncToSapAsync price
/// lookup). The original code did
/// <c>var itemKM = _context.Item.ToList();</c> before each per-doc sync
/// flow, then <c>itemKM.FirstOrDefault(...)</c> for a handful of
/// promotion lines. The fix narrows the load to the codes that actually
/// appear in the doc's promotions.
/// </summary>
public class SyncToSapPriceLookupBench : BenchmarkBase
{
    public SyncToSapPriceLookupBench(ITestOutputHelper output) : base(output) { }

    private const int Iterations = 5;
    private const double MinSpeedupRatio = 2.0;

    [Fact(Skip = "Perf benchmark — set APSP_BENCH_CONN env var and remove Skip to run.")]
    public async Task Filtered_load_should_outperform_full_table_load()
    {
        try { SkipIfUnconfigured(); }
        catch (SkipException) { return; }

        // Pick ~5 ItemCodes that actually exist — simulates promotion line ItemCodes.
        await using var probe = NewContext();
        var promoCodes = await probe.Item
            .AsNoTracking()
            .OrderBy(i => i.Id)
            .Take(5)
            .Select(i => i.ItemCode)
            .ToListAsync();

        var itemRowCount = await probe.Item.CountAsync();
        Output.WriteLine($"Item rows in DB: {itemRowCount:N0}; sample promo codes: {string.Join(", ", promoCodes)}");

        if (itemRowCount < 1_000)
        {
            Output.WriteLine($"[warning] Item table has only {itemRowCount} rows — gap will be small.");
        }

        // BEFORE — full table load + in-memory FirstOrDefault per code.
        var beforeMedian = await MeasureMedianAsync("BEFORE  Item.ToList() + FirstOrDefault", Iterations, async () =>
        {
            await using var ctx = NewContext();
            var itemKM = await ctx.Item.ToListAsync();
            foreach (var code in promoCodes)
            {
                _ = itemKM.FirstOrDefault(e => e.ItemCode == code)?.Price ?? 0;
            }
        });

        // AFTER — filtered AsNoTracking load on just the codes we need.
        var afterMedian = await MeasureMedianAsync("AFTER   AsNoTracking + WHERE IN", Iterations, async () =>
        {
            await using var ctx = NewContext();
            var itemKM = await ctx.Item
                .AsNoTracking()
                .Where(i => promoCodes.Contains(i.ItemCode))
                .ToListAsync();
            foreach (var code in promoCodes)
            {
                _ = itemKM.FirstOrDefault(e => e.ItemCode == code)?.Price ?? 0;
            }
        });

        var ratio = beforeMedian.TotalMilliseconds / Math.Max(afterMedian.TotalMilliseconds, 0.01);
        Output.WriteLine($"Speedup: {ratio:F1}x (BEFORE / AFTER, higher is better)");

        // Sanity assertion: AFTER must not be slower than BEFORE; expect a
        // material speedup once the dataset is realistic. Lower the
        // threshold if your local DB is small.
        afterMedian.Should().BeLessThanOrEqualTo(beforeMedian,
            "the filtered query loads strictly less data, so AFTER must not exceed BEFORE");

        if (itemRowCount >= 10_000)
        {
            ratio.Should().BeGreaterThan(MinSpeedupRatio,
                $"with {itemRowCount:N0} rows in OITM the AFTER path should be at least {MinSpeedupRatio}x faster");
        }
    }
}
