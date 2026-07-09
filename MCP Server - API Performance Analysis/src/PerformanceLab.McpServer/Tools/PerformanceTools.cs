using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;
using PerformanceLab.McpServer.Services;
using PerformanceLab.Shared.Models;

namespace PerformanceLab.McpServer.Tools;

[McpServerToolType]
public sealed class PerformanceTools(
    LoadTestRunner runner,
    ResultAnalyzer analyzer,
    IResultStore   store,
    ILogger<PerformanceTools> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    // ─────────────────────────────────────────────────────────────────────────
    //  run_load_test
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "run_load_test")]
    [Description(
        "Run a load test against an API endpoint. Fires concurrent HTTP requests for the " +
        "specified duration and returns throughput, latency percentiles, and error rate. " +
        "Returns a result ID that can be passed to analyze_results or generate_report.")]
    public async Task<string> RunLoadTest(
        [Description("Full URL of the endpoint to test. Example: http://localhost:5100/fast")]
        string url,

        [Description("How many seconds the test should run. Default: 10")]
        int durationSeconds = 10,

        [Description("Number of virtual concurrent users (parallel HTTP connections). Default: 10")]
        int concurrentUsers = 10,

        [Description("HTTP method: GET or POST. Default: GET")]
        string method = "GET",

        [Description("Optional JSON body for POST requests")]
        string? body = null,

        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool: run_load_test → {Url}", url);

        var request = new LoadTestRequest
        {
            Url             = url,
            DurationSeconds = durationSeconds,
            ConcurrentUsers = concurrentUsers,
            Method          = method,
            Body            = body,
        };

        LoadTestResult result;
        try
        {
            result = await runner.RunAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return $"❌ Load test failed: {ex.Message}";
        }

        var analysis = analyzer.Analyze(result);
        store.Add(result, analysis);

        return FormatLoadTestResult(result, analysis);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  compare_endpoints
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "compare_endpoints")]
    [Description(
        "Run a load test against two endpoints and compare the results side by side. " +
        "Shows percentage improvement or regression in RPS, average latency, p95, p99, and error rate. " +
        "Ideal for comparing Thread.Sleep vs Task.Delay, or sequential vs parallel implementations.")]
    public async Task<string> CompareEndpoints(
        [Description("URL of the baseline endpoint (the current or slower implementation)")]
        string baselineUrl,

        [Description("URL of the candidate endpoint (the new or faster implementation)")]
        string candidateUrl,

        [Description("How many seconds each test should run. Default: 10")]
        int durationSeconds = 10,

        [Description("Number of virtual concurrent users for each test. Default: 10")]
        int concurrentUsers = 10,

        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool: compare_endpoints → {A} vs {B}", baselineUrl, candidateUrl);

        var baselineReq  = new LoadTestRequest { Url = baselineUrl,  DurationSeconds = durationSeconds, ConcurrentUsers = concurrentUsers };
        var candidateReq = new LoadTestRequest { Url = candidateUrl, DurationSeconds = durationSeconds, ConcurrentUsers = concurrentUsers };

        LoadTestResult baseline, candidate;
        try
        {
            logger.LogInformation("Running baseline test...");
            baseline = await runner.RunAsync(baselineReq, cancellationToken);

            logger.LogInformation("Running candidate test...");
            candidate = await runner.RunAsync(candidateReq, cancellationToken);
        }
        catch (Exception ex)
        {
            return $"❌ Comparison failed: {ex.Message}";
        }

        var baselineAnalysis  = analyzer.Analyze(baseline);
        var candidateAnalysis = analyzer.Analyze(candidate);
        var comparison        = analyzer.Compare(baseline, candidate);

        store.Add(baseline,  baselineAnalysis);
        store.Add(candidate, candidateAnalysis);

        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════");
        sb.AppendLine("  ENDPOINT COMPARISON");
        sb.AppendLine("═══════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine(comparison.Summary);
        sb.AppendLine();
        sb.AppendLine("Baseline result ID  : " + baseline.Id);
        sb.AppendLine("Candidate result ID : " + candidate.Id);
        sb.AppendLine();
        sb.AppendLine("Use generate_report with these IDs for a full markdown report.");

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  analyze_results
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "analyze_results")]
    [Description(
        "Analyze a previous load test result by its ID and get a diagnosis. " +
        "Detects slow responses, threadpool starvation symptoms, high error rates, " +
        "latency variance, and GC pressure. Returns severity, issues found, and " +
        "concrete .NET recommendations. Pass the result ID returned by run_load_test.")]
    public string AnalyzeResults(
        [Description("The result ID returned by run_load_test or compare_endpoints")]
        string resultId)
    {
        logger.LogInformation("Tool: analyze_results → {Id}", resultId);

        var run = store.GetById(resultId);
        if (run is null)
            return $"❌ No result found with ID '{resultId}'. Run a load test first with run_load_test.";

        return FormatAnalysis(run.Result, run.Analysis);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  detect_slow_responses
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "detect_slow_responses")]
    [Description(
        "Check a load test result for slow response patterns. Returns whether the endpoint " +
        "has slow average latency, high p95/p99 tail latency, or poor throughput.")]
    public string DetectSlowResponses(
        [Description("The result ID to inspect")]
        string resultId,

        [Description("Latency threshold in milliseconds to consider 'slow'. Default: 500ms")]
        int thresholdMs = 500)
    {
        var run = store.GetById(resultId);
        if (run is null)
            return $"❌ No result found with ID '{resultId}'.";

        var r     = run.Result;
        var flags = new List<string>();

        if (r.AverageResponseTimeMs > thresholdMs)
            flags.Add($"Average latency {r.AverageResponseTimeMs:F0}ms exceeds threshold {thresholdMs}ms");
        if (r.P95Ms > thresholdMs * 3)
            flags.Add($"p95 latency {r.P95Ms:F0}ms is over 3× the threshold");
        if (r.P99Ms > thresholdMs * 5)
            flags.Add($"p99 latency {r.P99Ms:F0}ms is over 5× the threshold");
        if (r.MaxMs > thresholdMs * 20)
            flags.Add($"Max latency {r.MaxMs:F0}ms spike detected (possible GC pause)");

        if (flags.Count == 0)
            return $"✅ No slow responses detected. All latencies are within threshold ({thresholdMs}ms).";

        return "⚠️  Slow response indicators found:\n" + string.Join("\n", flags.Select(f => $"  • {f}"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  detect_threadpool_starvation
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "detect_threadpool_starvation")]
    [Description(
        "Analyze a load test result for ThreadPool starvation symptoms. " +
        "Starvation occurs when all ThreadPool threads are blocked (e.g. by Thread.Sleep or .Result calls) " +
        "and new requests queue up. The signature is a very large gap between p50 and p99 latency.")]
    public string DetectThreadPoolStarvation(
        [Description("The result ID to inspect")]
        string resultId)
    {
        var run = store.GetById(resultId);
        if (run is null)
            return $"❌ No result found with ID '{resultId}'.";

        var r     = run.Result;
        var sb    = new StringBuilder();
        var ratio = r.P50Ms > 0 ? r.P99Ms / r.P50Ms : 0;

        sb.AppendLine("═══════ ThreadPool Starvation Analysis ═══════");
        sb.AppendLine($"URL         : {r.Url}");
        sb.AppendLine($"Concurrent  : {r.ConcurrentUsers} users");
        sb.AppendLine($"p50 latency : {r.P50Ms:F0}ms  (what most requests experience)");
        sb.AppendLine($"p99 latency : {r.P99Ms:F0}ms  (worst 1% of requests)");
        sb.AppendLine($"p99/p50 ratio: {ratio:F1}×");
        sb.AppendLine();

        if (ratio > 20)
        {
            sb.AppendLine("🔴 STARVATION DETECTED — p99 is more than 20× higher than p50.");
            sb.AppendLine();
            sb.AppendLine("This is the classic threadpool starvation pattern:");
            sb.AppendLine("  • Most requests complete quickly (p50 is low)");
            sb.AppendLine("  • But some requests wait a very long time (p99 is huge)");
            sb.AppendLine("  • The waiting requests are stuck in a queue because all threads are blocked");
            sb.AppendLine();
            sb.AppendLine("Most likely cause: Thread.Sleep() or blocking .Result/.Wait() calls.");
            sb.AppendLine("Fix: Replace Thread.Sleep with await Task.Delay. Replace .Result with await.");
        }
        else if (ratio > 5)
        {
            sb.AppendLine("🟡 VARIANCE WARNING — p99 is more than 5× higher than p50.");
            sb.AppendLine("Some requests are significantly slower than the median. Possible early-stage starvation.");
        }
        else
        {
            sb.AppendLine("✅ No starvation detected. The p99/p50 ratio looks healthy.");
        }

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  detect_memory_pressure
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "detect_memory_pressure")]
    [Description(
        "Analyze a load test result for signs of GC / memory pressure. " +
        "Infers pressure from unusual spikes in max latency relative to p99, " +
        "which is a common symptom of Gen2 garbage collection pauses.")]
    public string DetectMemoryPressure(
        [Description("The result ID to inspect")]
        string resultId)
    {
        var run = store.GetById(resultId);
        if (run is null)
            return $"❌ No result found with ID '{resultId}'.";

        var r  = run.Result;
        var sb = new StringBuilder();

        sb.AppendLine("═══════ Memory / GC Pressure Analysis ═══════");
        sb.AppendLine($"URL       : {r.Url}");
        sb.AppendLine($"p99       : {r.P99Ms:F0}ms");
        sb.AppendLine($"Max       : {r.MaxMs:F0}ms");
        sb.AppendLine($"Spike ratio (Max/p99): {(r.P99Ms > 0 ? r.MaxMs / r.P99Ms : 0):F1}×");
        sb.AppendLine();

        if (r.P99Ms > 0 && r.MaxMs > r.P99Ms * 3)
        {
            sb.AppendLine("🔴 GC PAUSE SUSPECTED");
            sb.AppendLine($"The max response time ({r.MaxMs:F0}ms) is more than 3× the p99 ({r.P99Ms:F0}ms).");
            sb.AppendLine("This spike pattern often indicates a Gen2 GC pause blocking all managed threads.");
            sb.AppendLine();
            sb.AppendLine("Likely causes:");
            sb.AppendLine("  • Allocating many short-lived large objects per request (e.g. new byte[1MB])");
            sb.AppendLine("  • Large LINQ chains producing intermediate collections");
            sb.AppendLine("  • String concatenation in a loop (use StringBuilder instead)");
            sb.AppendLine();
            sb.AppendLine("Fixes:");
            sb.AppendLine("  • Use ArrayPool<T>.Shared.Rent() instead of new byte[]");
            sb.AppendLine("  • Use Span<T> and stackalloc for small buffers");
            sb.AppendLine("  • Profile with dotnet-trace: dotnet-trace collect -p <pid> --profile gc-verbose");
        }
        else if (r.P99Ms > 0 && r.MaxMs > r.P99Ms * 1.5)
        {
            sb.AppendLine("🟡 MINOR SPIKES — Max is moderately higher than p99. Could be occasional GC or OS scheduling.");
        }
        else
        {
            sb.AppendLine("✅ No significant memory pressure indicators. Max latency is close to p99.");
        }

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  compare_before_after
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "compare_before_after")]
    [Description(
        "Compare two previously stored load test results by their IDs. " +
        "Useful for measuring the impact of a code change: run a test before, make the change, " +
        "run again, then compare. Returns a full diff of all key metrics.")]
    public string CompareBeforeAfter(
        [Description("The result ID of the 'before' test run")]
        string beforeId,

        [Description("The result ID of the 'after' test run")]
        string afterId)
    {
        var before = store.GetById(beforeId);
        var after  = store.GetById(afterId);

        if (before is null) return $"❌ No result found with ID '{beforeId}'.";
        if (after  is null) return $"❌ No result found with ID '{afterId}'.";

        var comparison = analyzer.Compare(before.Result, after.Result);

        var sb = new StringBuilder();
        sb.AppendLine("═══════ Before / After Comparison ═══════");
        sb.AppendLine($"BEFORE : {before.Result.Url} (ID: {beforeId})");
        sb.AppendLine($"AFTER  : {after.Result.Url} (ID: {afterId})");
        sb.AppendLine();
        sb.AppendLine(comparison.Summary);

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  generate_report
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "generate_report")]
    [Description(
        "Generate a human-readable markdown performance report for one or more test result IDs. " +
        "Includes summary table, latency breakdown, diagnosis, and recommendations. " +
        "Pass a comma-separated list of result IDs (e.g. 'abc123,def456').")]
    public string GenerateReport(
        [Description("Comma-separated list of result IDs to include in the report")]
        string resultIds,

        [Description("Optional title for the report. Default: 'Performance Lab Report'")]
        string title = "Performance Lab Report")
    {
        var ids  = resultIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var runs = ids.Select(id => store.GetById(id)).Where(r => r is not null).ToList();

        if (runs.Count == 0)
            return $"❌ No results found for the provided IDs: {resultIds}";

        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine($"> Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Summary table
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("| Endpoint | RPS | Avg (ms) | P50 (ms) | P95 (ms) | P99 (ms) | Error % | Severity |");
        sb.AppendLine("|----------|-----|----------|----------|----------|----------|---------|----------|");

        foreach (var run in runs)
        {
            var r = run!.Result;
            var a = run.Analysis;
            sb.AppendLine(
                $"| `{r.Label}` | {r.RequestsPerSecond:F1} | {r.AverageResponseTimeMs:F0} | " +
                $"{r.P50Ms:F0} | {r.P95Ms:F0} | {r.P99Ms:F0} | {r.ErrorRate:F1}% | **{a.Severity}** |");
        }

        sb.AppendLine();

        // Detailed section per result
        foreach (var run in runs)
        {
            var r = run!.Result;
            var a = run.Analysis;

            sb.AppendLine($"## {r.Label}");
            sb.AppendLine();
            sb.AppendLine($"- **URL:** `{r.Url}`");
            sb.AppendLine($"- **Duration:** {r.DurationSeconds}s | **Concurrent Users:** {r.ConcurrentUsers}");
            sb.AppendLine($"- **Timestamp:** {r.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            sb.AppendLine("### Metrics");
            sb.AppendLine();
            sb.AppendLine($"| Metric | Value |");
            sb.AppendLine($"|--------|-------|");
            sb.AppendLine($"| Total Requests | {r.TotalRequests:N0} |");
            sb.AppendLine($"| Successful | {r.SuccessfulRequests:N0} |");
            sb.AppendLine($"| Failed | {r.FailedRequests:N0} |");
            sb.AppendLine($"| Requests/sec (RPS) | **{r.RequestsPerSecond:F1}** |");
            sb.AppendLine($"| Average latency | **{r.AverageResponseTimeMs:F0}ms** |");
            sb.AppendLine($"| P50 (median) | {r.P50Ms:F0}ms |");
            sb.AppendLine($"| P95 | {r.P95Ms:F0}ms |");
            sb.AppendLine($"| P99 | {r.P99Ms:F0}ms |");
            sb.AppendLine($"| Min | {r.MinMs:F0}ms |");
            sb.AppendLine($"| Max | {r.MaxMs:F0}ms |");
            sb.AppendLine($"| Error Rate | {r.ErrorRate:F1}% |");
            sb.AppendLine();

            sb.AppendLine($"### Diagnosis — Severity: **{a.Severity}**");
            sb.AppendLine();
            foreach (var issue in a.Issues)
                sb.AppendLine($"- {issue}");
            sb.AppendLine();

            if (a.Recommendations.Count > 0)
            {
                sb.AppendLine("### Recommendations");
                sb.AppendLine();
                foreach (var rec in a.Recommendations)
                    sb.AppendLine($"- {rec}");
                sb.AppendLine();
            }

            if (a.Optimizations.Count > 0)
            {
                sb.AppendLine("### .NET Optimizations");
                sb.AppendLine();
                foreach (var opt in a.Optimizations)
                    sb.AppendLine($"- `{opt}`");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  suggest_optimizations
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "suggest_optimizations")]
    [Description(
        "Get practical .NET optimization suggestions for a specific issue pattern. " +
        "Supported patterns: 'threadpool', 'memory', 'database', 'cpu', 'general'")]
    public string SuggestOptimizations(
        [Description("The pattern to get suggestions for: threadpool, memory, database, cpu, or general")]
        string pattern = "general")
    {
        return pattern.ToLowerInvariant() switch
        {
            "threadpool" => """
                ## ThreadPool Starvation — Fixes

                **Root cause:** Blocked threads prevent the pool from handling new requests.

                **Immediate fixes:**
                - Replace `Thread.Sleep(ms)` with `await Task.Delay(ms)`
                - Replace `.Result` and `.Wait()` with `await`
                - Replace `Task.Run(...).Result` with `await Task.Run(...)`
                - Replace synchronous `File.ReadAllText` with `await File.ReadAllTextAsync`

                **Diagnostic tools:**
                ```bash
                # See current threadpool queue length and thread count
                dotnet-counters monitor --counters System.Runtime
                
                # Look for threadpool starvation events
                dotnet-trace collect -p <pid> --profile cpu-sampling
                ```

                **Architecture fix:**
                Move CPU-bound or blocking work to a dedicated background queue
                (e.g. System.Threading.Channels + IHostedService) instead of processing
                it inline on request threads.
                """,

            "memory" => """
                ## Memory / GC Pressure — Fixes

                **Root cause:** Many short-lived large allocations trigger frequent GC pauses.

                **Immediate fixes:**
                - Replace `new byte[n]` with `ArrayPool<byte>.Shared.Rent(n)`
                  (remember to Return() it when done)
                - Use `Span<T>` and `Memory<T>` to slice existing arrays without copying
                - Replace `string.Concat` in loops with `StringBuilder`
                - Add `.AsNoTracking()` to read-only EF Core queries

                **Profiling:**
                ```bash
                dotnet-trace collect -p <pid> --profile gc-verbose
                dotnet-gcdump collect -p <pid>
                ```

                Open the .gcdump in Visual Studio or PerfView to see
                what's allocated and where.
                """,

            "database" => """
                ## Database Performance — Fixes

                **N+1 query problem:**
                ```csharp
                // BAD — 1 query per item
                foreach (var order in orders)
                    order.Items = await db.Items.Where(i => i.OrderId == order.Id).ToListAsync();

                // GOOD — 1 query for everything
                var allItems = await db.Items
                    .Where(i => orders.Select(o => o.Id).Contains(i.OrderId))
                    .ToListAsync();
                ```

                **Parallel independent queries:**
                ```csharp
                // BAD — sequential
                var users    = await db.Users.ToListAsync();
                var products = await db.Products.ToListAsync();

                // GOOD — parallel
                var (users, products) = await (
                    db.Users.ToListAsync(),
                    db.Products.ToListAsync()
                ).WhenAll();
                ```

                **EF Core compiled queries (hot paths):**
                ```csharp
                private static readonly Func<AppDbContext, int, Task<User?>> GetUserById =
                    EF.CompileAsyncQuery((AppDbContext ctx, int id) =>
                        ctx.Users.FirstOrDefault(u => u.Id == id));
                ```
                """,

            "cpu" => """
                ## CPU-Bound Work — Fixes

                **Move to background thread:**
                ```csharp
                // Never on a request thread:
                app.MapGet("/report", () => {
                    var pdf = GenerateHugePdf(); // 3 seconds of CPU
                    return pdf;
                });

                // Correct — offload to thread pool:
                app.MapGet("/report", async () => {
                    var pdf = await Task.Run(() => GenerateHugePdf());
                    return pdf;
                });
                ```

                **Queue long work:**
                For work > 1-2s, don't make the HTTP request wait at all.
                Accept the request, enqueue the job (System.Threading.Channels or
                a message bus), return a job ID, and poll/webhook when done.

                **Parallelize CPU work:**
                ```csharp
                var results = await Task.WhenAll(
                    Task.Run(() => ProcessChunk(data[..half])),
                    Task.Run(() => ProcessChunk(data[half..]))
                );
                ```
                """,

            _ => """
                ## General .NET API Performance Checklist

                **Async all the way:**
                - Use `async/await` from controller/endpoint down to the I/O call
                - Never mix `async` code with `.Result`, `.Wait()`, or `Thread.Sleep`

                **Response caching:**
                ```csharp
                builder.Services.AddOutputCache();
                app.UseOutputCache();
                app.MapGet("/data", Handler).CacheOutput(p => p.Expire(TimeSpan.FromMinutes(1)));
                ```

                **Connection pooling:**
                - Register `HttpClient` via `IHttpClientFactory`, never `new HttpClient()`
                - For EF Core: use a singleton `DbContextPool` in high-throughput apps

                **JSON performance:**
                ```csharp
                // Use source generation to avoid runtime reflection
                [JsonSerializable(typeof(MyResponse))]
                public partial class MyJsonContext : JsonSerializerContext {}
                ```

                **Minimal allocations:**
                - Prefer `IAsyncEnumerable<T>` over `List<T>` for large result sets
                - Use `struct` for small, frequently allocated value objects
                - Enable `<AllowUnsafeBlocks>` only if you need unsafe/Span perf

                **Observability:**
                ```bash
                dotnet-counters monitor --counters System.Runtime,Microsoft.AspNetCore.Hosting
                ```
                """
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  list_results
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "list_results")]
    [Description("List all stored load test results with their IDs, URLs, and key metrics. Use result IDs with analyze_results or generate_report.")]
    public string ListResults()
    {
        var runs = store.GetAll();
        if (runs.Count == 0)
            return "No results stored yet. Run a load test with run_load_test first.";

        var sb = new StringBuilder();
        sb.AppendLine($"Stored results ({runs.Count} total):");
        sb.AppendLine();

        foreach (var run in runs)
        {
            var r = run.Result;
            sb.AppendLine($"ID: {r.Id} | {r.Label,-30} | RPS: {r.RequestsPerSecond,6:F1} | Avg: {r.AverageResponseTimeMs,6:F0}ms | p95: {r.P95Ms,6:F0}ms | Severity: {run.Analysis.Severity}");
        }

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string FormatLoadTestResult(LoadTestResult r, AnalysisResult a)
    {
        var sb = new StringBuilder();
        sb.AppendLine("✅ Load Test Complete");
        sb.AppendLine($"Result ID : {r.Id}");
        sb.AppendLine($"URL       : {r.Url}");
        sb.AppendLine($"Timestamp : {r.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("📊 Throughput");
        sb.AppendLine($"  Total Requests : {r.TotalRequests:N0}");
        sb.AppendLine($"  Successful     : {r.SuccessfulRequests:N0}");
        sb.AppendLine($"  Failed         : {r.FailedRequests:N0}");
        sb.AppendLine($"  RPS            : {r.RequestsPerSecond:F1}");
        sb.AppendLine();
        sb.AppendLine("⏱  Latency");
        sb.AppendLine($"  Average : {r.AverageResponseTimeMs,8:F1} ms");
        sb.AppendLine($"  P50     : {r.P50Ms,8:F1} ms");
        sb.AppendLine($"  P95     : {r.P95Ms,8:F1} ms");
        sb.AppendLine($"  P99     : {r.P99Ms,8:F1} ms");
        sb.AppendLine($"  Min     : {r.MinMs,8:F1} ms");
        sb.AppendLine($"  Max     : {r.MaxMs,8:F1} ms");
        sb.AppendLine($"  Errors  : {r.ErrorRate:F1}%");
        sb.AppendLine();
        sb.AppendLine($"🔍 Severity : {a.Severity}");

        if (a.Issues.Count > 0 && a.Issues[0] != "No significant performance issues detected.")
        {
            sb.AppendLine("Issues detected:");
            foreach (var issue in a.Issues)
                sb.AppendLine($"  • {issue}");
        }

        sb.AppendLine();
        sb.AppendLine($"Use analyze_results with ID '{r.Id}' for full diagnosis.");

        return sb.ToString();
    }

    private static string FormatAnalysis(LoadTestResult r, AnalysisResult a)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"🔍 Analysis for {r.Label}  (ID: {r.Id})");
        sb.AppendLine($"Severity: {a.Severity}");
        sb.AppendLine();

        sb.AppendLine("Issues:");
        foreach (var issue in a.Issues)
            sb.AppendLine($"  • {issue}");

        if (a.Recommendations.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Recommendations:");
            foreach (var rec in a.Recommendations)
                sb.AppendLine($"  → {rec}");
        }

        if (a.Optimizations.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(".NET Optimizations:");
            foreach (var opt in a.Optimizations)
                sb.AppendLine($"  ✦ {opt}");
        }

        return sb.ToString();
    }
}
