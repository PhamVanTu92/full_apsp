# Performance benchmarks

Skipped-by-default Stopwatch tests that compare the BEFORE / AFTER query
shapes from `BACKEND_REVIEW.md` H-3 and H-6 against a real SQL Server
database. They live in this folder so they share the test runner but
opt out of CI by carrying `[Fact(Skip = ...)]`.

## Why skipped by default

- They need a populated SQL Server database; in-memory EF provider does
  not reflect real query plan / IO behaviour and would defeat the
  purpose of measuring.
- They run in seconds-to-minutes, not milliseconds — too slow for the
  default unit-test loop.
- Numbers depend on table size; meaningful only when run against a
  dataset with realistic scale.

## How to run

1. Provision a SQL Server (local SQLEXPRESS, dev container, or a copy
   of UAT) and apply migrations:

   ```bash
   cd backend
   dotnet ef database update --project BackEndAPI \
     --connection "Server=.;Database=apsp_bench;Trusted_Connection=true;TrustServerCertificate=True"
   ```

2. Seed the relevant tables. The benchmarks will create their own
   throwaway rows when the table is empty, but for representative
   timing you want production-like sizes:

   | Table | Rows | Why |
   |---|---|---|
   | `OITM` (Item) | 50 000+ | H-3: full table scan vs filtered seek |
   | `WTM2` | 1 000+ | H-6: full list vs SQL EXISTS |
   | `OWTM` (with RUsers m2m) | 200+ | nav for H-6 |

   Restore from a UAT backup is the simplest path. If you only have a
   small DB, the benchmarks still run — they just won't show the
   difference because both shapes finish in <50 ms.

3. Set the connection string and run only this folder:

   ```bash
   export APSP_BENCH_CONN="Server=.;Database=apsp_bench;Trusted_Connection=true;TrustServerCertificate=True"
   dotnet test backend/BackEndAPI.Tests \
     --filter "FullyQualifiedName~Performance" \
     -- xunit.methodDisplay=method
   ```

   On Windows PowerShell:
   ```powershell
   $env:APSP_BENCH_CONN = "Server=.;Database=apsp_bench;Trusted_Connection=true;TrustServerCertificate=True"
   dotnet test backend/BackEndAPI.Tests --filter "FullyQualifiedName~Performance"
   ```

4. Remove the `Skip` attribute on the `[Fact]` (or change it to an
   empty string) — xUnit otherwise refuses to run skipped tests even
   under a filter. The benchmarks themselves only run when
   `APSP_BENCH_CONN` is set; the assertion will be inconclusive
   otherwise.

## What they measure

Each benchmark runs the BEFORE shape and the AFTER shape sequentially
against the same dataset, repeats N times (default 5), discards the
warm-up run, and reports the median. It then asserts that AFTER is
materially faster than BEFORE (default ratio threshold 2x — adjust if
your dataset is too small for a meaningful gap).

Output goes to `ITestOutputHelper`; to see it in the console run with
`-l "console;verbosity=detailed"`.

## Caveats

- These are **microbenchmarks against EF Core query shapes**, not full
  end-to-end profiles of `SyncToSapAsync` or `Approval.ActionApproval`.
  They isolate the line that the review flagged.
- They do not exercise the SQL Server query cache the same way prod
  does. First run after server restart may show inflated numbers —
  warm-up iteration is discarded specifically for this reason.
- For production rollout planning, complement these with
  `SET STATISTICS TIME, IO ON` runs in SSMS on a real workload.
