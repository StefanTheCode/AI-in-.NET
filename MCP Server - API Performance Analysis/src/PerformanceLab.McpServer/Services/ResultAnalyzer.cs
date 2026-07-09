using PerformanceLab.Shared.Models;

namespace PerformanceLab.McpServer.Services;

/// <summary>
/// Analyses a LoadTestResult and produces a diagnosis with actionable recommendations.
/// No AI required — pure rule-based analysis of the performance metrics.
/// </summary>
public sealed class ResultAnalyzer
{
    public AnalysisResult Analyze(LoadTestResult result)
    {
        var issues          = new List<string>();
        var recommendations = new List<string>();
        var optimizations   = new List<string>();

        // ── Latency checks ────────────────────────────────────────────────────
        if (result.AverageResponseTimeMs > 1000)
        {
            issues.Add($"Average response time is {result.AverageResponseTimeMs:F0}ms — over 1 second.");
            recommendations.Add("An average over 1s usually means a blocking call, slow database query, or an external HTTP call on the hot path.");
        }
        else if (result.AverageResponseTimeMs > 300)
        {
            issues.Add($"Average response time is {result.AverageResponseTimeMs:F0}ms — acceptable but worth investigating.");
        }

        if (result.P95Ms > 2000)
        {
            issues.Add($"p95 latency is {result.P95Ms:F0}ms. 1 in 20 requests takes over 2 seconds.");
            recommendations.Add("High p95 latency degrades the experience for real users. Check for GC pauses, long lock contention, or slow downstream dependencies.");
        }
        else if (result.P95Ms > 500)
        {
            issues.Add($"p95 latency is {result.P95Ms:F0}ms. Worth investigating.");
        }

        if (result.P99Ms > 5000)
        {
            issues.Add($"p99 latency is {result.P99Ms:F0}ms — 5+ seconds. This is a critical tail-latency problem.");
            recommendations.Add("5-second p99 is severe. Look for uncontrolled retry storms, lack of timeouts, or GC Gen2 collections blocking all threads.");
        }

        // ── Thread pool starvation signal ────────────────────────────────────
        // A large gap between p50 and p99 is the classic starvation signature.
        // Healthy APIs have p99/p50 < 5. Starved APIs show p99/p50 of 20–100×.
        if (result.P50Ms > 0 && result.P99Ms / result.P50Ms > 10)
        {
            var ratio = result.P99Ms / result.P50Ms;
            issues.Add($"High latency variance detected — p99 is {ratio:F0}× higher than p50. Classic threadpool starvation signature.");
            recommendations.Add(
                "When p99 is 10–100× higher than p50, most requests are fast but a few are catastrophically slow. " +
                "This is what ThreadPool starvation looks like: new requests queue up waiting for a thread to become free.");
            recommendations.Add(
                "Search your codebase for: Thread.Sleep(), .Result, .Wait(), GetAwaiter().GetResult(), and any " +
                "synchronous file or network I/O on async threads. Replace them all with async/await equivalents.");
            optimizations.Add("Replace Thread.Sleep with await Task.Delay.");
            optimizations.Add("Replace .Result / .Wait() with await.");
            optimizations.Add("Replace HttpWebRequest with HttpClient and await.");
            optimizations.Add("If you must do CPU-bound work on a request thread, wrap it in Task.Run to avoid starving the ThreadPool.");
        }

        // ── Throughput checks ─────────────────────────────────────────────────
        if (result.RequestsPerSecond < 5 && result.ConcurrentUsers >= 10)
        {
            issues.Add($"Very low throughput: {result.RequestsPerSecond:F1} RPS with {result.ConcurrentUsers} concurrent users.");
            recommendations.Add("Under 5 RPS with 10 concurrent users suggests severe blocking. Check for Thread.Sleep or external dependencies with no timeout.");
        }

        // ── Error rate checks ─────────────────────────────────────────────────
        if (result.ErrorRate > 10)
        {
            issues.Add($"Error rate is {result.ErrorRate:F1}%. Over 10% of requests are failing.");
            recommendations.Add("High error rates often indicate: connection pool exhaustion, server overload, missing cancellation token handling, or requests timing out.");
            optimizations.Add("Add a timeout to your HttpClient (e.g. client.Timeout = TimeSpan.FromSeconds(5)).");
            optimizations.Add("Implement circuit breakers with Polly to stop hammering a failing dependency.");
        }
        else if (result.ErrorRate > 1)
        {
            issues.Add($"Error rate is {result.ErrorRate:F1}% — worth investigating.");
            recommendations.Add("Even 1–5% errors matter in production. Check server logs for the root cause.");
        }

        // ── Memory pressure hints (inferred from response time pattern) ───────
        if (result.MaxMs > result.P99Ms * 3)
        {
            issues.Add($"Max response time ({result.MaxMs:F0}ms) is more than 3× the p99 ({result.P99Ms:F0}ms). Suggests occasional GC pauses.");
            recommendations.Add("Large spikes beyond p99 are often caused by Gen2 garbage collection pauses. Profile allocations with dotnet-trace or BenchmarkDotNet.");
            optimizations.Add("Use ArrayPool<T> or MemoryPool<T> to reuse buffers.");
            optimizations.Add("Avoid LINQ chains that produce many intermediate collections in hot paths.");
            optimizations.Add("Consider using Span<T> and stackalloc for small, short-lived data.");
        }

        // ── All clear ─────────────────────────────────────────────────────────
        if (issues.Count == 0)
        {
            issues.Add("No significant performance issues detected.");
            recommendations.Add("Results look healthy. Run the test with more concurrent users or longer duration to find limits.");
        }

        // ── General .NET optimizations always worth mentioning ────────────────
        if (optimizations.Count == 0)
        {
            optimizations.Add("Enable response compression (app.UseResponseCompression()).");
            optimizations.Add("Use output caching for endpoints that return the same data frequently.");
            optimizations.Add("Add an EF Core compiled query for frequently executed LINQ queries.");
        }

        return new AnalysisResult
        {
            ResultId        = result.Id,
            Url             = result.Url,
            Issues          = issues,
            Recommendations = recommendations,
            Optimizations   = optimizations,
            Severity        = DetermineSeverity(result),
        };
    }

    public CompareResult Compare(LoadTestResult baseline, LoadTestResult candidate)
    {
        static double PercentChange(double before, double after) =>
            before == 0 ? 0 : (after - before) / before * 100;

        var rpsChange       = PercentChange(baseline.RequestsPerSecond,     candidate.RequestsPerSecond);
        var avgChange       = PercentChange(baseline.AverageResponseTimeMs, candidate.AverageResponseTimeMs);
        var p95Change       = PercentChange(baseline.P95Ms,                 candidate.P95Ms);
        var p99Change       = PercentChange(baseline.P99Ms,                 candidate.P99Ms);
        var errorRateDiff   = candidate.ErrorRate - baseline.ErrorRate;

        // Candidate is "better" if it's faster (lower latency, higher RPS) and not significantly more errors
        var isFaster = avgChange < -5 || rpsChange > 5;

        var summary = BuildComparisonSummary(
            baseline, candidate, rpsChange, avgChange, p95Change, p99Change, errorRateDiff, isFaster);

        return new CompareResult
        {
            Baseline              = baseline,
            Candidate             = candidate,
            RpsChangePercent      = rpsChange,
            AvgLatencyChangePercent = avgChange,
            P95ChangePercent      = p95Change,
            P99ChangePercent      = p99Change,
            ErrorRateAbsoluteDiff = errorRateDiff,
            IsCandidateFaster     = isFaster,
            Winner                = isFaster ? "Candidate" : "Baseline",
            Summary               = summary,
        };
    }

    private static string BuildComparisonSummary(
        LoadTestResult baseline, LoadTestResult candidate,
        double rpsChange, double avgChange, double p95Change, double p99Change,
        double errorRateDiff, bool isFaster)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"BASELINE  → {baseline.Url}");
        sb.AppendLine($"CANDIDATE → {candidate.Url}");
        sb.AppendLine();
        sb.AppendLine("THROUGHPUT");
        sb.AppendLine($"  Baseline RPS  : {baseline.RequestsPerSecond:F1}");
        sb.AppendLine($"  Candidate RPS : {candidate.RequestsPerSecond:F1}  ({FormatChange(rpsChange, higherIsBetter: true)})");
        sb.AppendLine();
        sb.AppendLine("LATENCY");
        sb.AppendLine($"  Average  — baseline: {baseline.AverageResponseTimeMs:F0}ms → candidate: {candidate.AverageResponseTimeMs:F0}ms  ({FormatChange(avgChange, higherIsBetter: false)})");
        sb.AppendLine($"  P95      — baseline: {baseline.P95Ms:F0}ms → candidate: {candidate.P95Ms:F0}ms  ({FormatChange(p95Change, higherIsBetter: false)})");
        sb.AppendLine($"  P99      — baseline: {baseline.P99Ms:F0}ms → candidate: {candidate.P99Ms:F0}ms  ({FormatChange(p99Change, higherIsBetter: false)})");
        sb.AppendLine();
        sb.AppendLine("ERRORS");
        sb.AppendLine($"  Baseline error rate  : {baseline.ErrorRate:F1}%");
        sb.AppendLine($"  Candidate error rate : {candidate.ErrorRate:F1}%  (diff: {errorRateDiff:+0.0;-0.0;0.0}pp)");
        sb.AppendLine();
        sb.AppendLine($"VERDICT: {(isFaster ? "✅ Candidate is faster" : "⚠️  Baseline is equal or better")}");

        if (Math.Abs(avgChange) >= 50)
        {
            sb.AppendLine();
            sb.AppendLine(avgChange < 0
                ? $"🔑 The candidate is {Math.Abs(avgChange):F0}% faster on average latency. Significant improvement."
                : $"⚠️  The candidate is {Math.Abs(avgChange):F0}% SLOWER on average latency. Regression detected.");
        }

        return sb.ToString();
    }

    private static string FormatChange(double percent, bool higherIsBetter)
    {
        var improved = higherIsBetter ? percent > 0 : percent < 0;
        var sign     = percent >= 0 ? "+" : "";
        var icon     = improved ? "✅" : (Math.Abs(percent) < 2 ? "≈" : "❌");
        return $"{icon} {sign}{percent:F1}%";
    }

    private static string DetermineSeverity(LoadTestResult r)
    {
        if (r.ErrorRate > 10 || r.P99Ms > 10_000)                                             return "Critical";
        if (r.ErrorRate > 5  || r.P95Ms > 3000 || r.AverageResponseTimeMs > 1000)             return "High";
        if (r.ErrorRate > 1  || r.P95Ms > 1000 || r.AverageResponseTimeMs > 500)              return "Medium";
        return "Low";
    }
}
