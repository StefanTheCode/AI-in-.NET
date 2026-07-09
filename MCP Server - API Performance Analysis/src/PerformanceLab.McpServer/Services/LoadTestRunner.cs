using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using PerformanceLab.Shared.Models;

namespace PerformanceLab.McpServer.Services;

/// <summary>
/// Runs HTTP load tests against a target URL using multiple concurrent virtual users.
/// Uses plain .NET HttpClient — no external tools required.
/// </summary>
public sealed class LoadTestRunner
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoadTestRunner> _logger;

    public LoadTestRunner(IHttpClientFactory httpClientFactory, ILogger<LoadTestRunner> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    public async Task<LoadTestResult> RunAsync(LoadTestRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting load test — URL: {Url} | Duration: {Duration}s | Users: {Users}",
            request.Url, request.DurationSeconds, request.ConcurrentUsers);

        var records   = new ConcurrentBag<RequestRecord>();
        var endAt     = DateTime.UtcNow.AddSeconds(request.DurationSeconds);
        var totalSw   = Stopwatch.StartNew();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(request.DurationSeconds + 15));

        var workerTasks = Enumerable
            .Range(0, request.ConcurrentUsers)
            .Select(_ => RunWorkerAsync(request, records, endAt, linkedCts.Token))
            .ToArray();

        await Task.WhenAll(workerTasks);
        totalSw.Stop();

        _logger.LogInformation(
            "Load test complete — {Total} requests in {Elapsed:F1}s",
            records.Count, totalSw.Elapsed.TotalSeconds);

        return BuildResult(request, [.. records], totalSw.Elapsed);
    }

    private async Task RunWorkerAsync(
        LoadTestRequest request,
        ConcurrentBag<RequestRecord> records,
        DateTime endAt,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("loadtest");

        while (DateTime.UtcNow < endAt && !ct.IsCancellationRequested)
        {
            var reqSw = Stopwatch.StartNew();
            try
            {
                HttpResponseMessage response;

                if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                    && request.Body is not null)
                {
                    using var content = new StringContent(request.Body, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(request.Url, content, ct);
                }
                else
                {
                    response = await client.GetAsync(request.Url, ct);
                }

                reqSw.Stop();
                records.Add(new RequestRecord(reqSw.Elapsed.TotalMilliseconds, response.IsSuccessStatusCode, null));
                response.Dispose();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                reqSw.Stop();
                records.Add(new RequestRecord(reqSw.Elapsed.TotalMilliseconds, false, ex.GetType().Name));
                _logger.LogDebug("Request failed: {Error}", ex.Message);
            }
        }
    }

    private static LoadTestResult BuildResult(
        LoadTestRequest request,
        List<RequestRecord> records,
        TimeSpan elapsed)
    {
        if (records.Count == 0)
        {
            return new LoadTestResult
            {
                Url              = request.Url,
                Label            = LabelFromUrl(request.Url),
                DurationSeconds  = request.DurationSeconds,
                ConcurrentUsers  = request.ConcurrentUsers,
            };
        }

        var times      = records.Select(r => r.ElapsedMs).OrderBy(x => x).ToList();
        var successful = records.Count(r => r.Success);
        var failed     = records.Count(r => !r.Success);

        return new LoadTestResult
        {
            Url                    = request.Url,
            Label                  = LabelFromUrl(request.Url),
            DurationSeconds        = request.DurationSeconds,
            ConcurrentUsers        = request.ConcurrentUsers,
            TotalRequests          = records.Count,
            SuccessfulRequests     = successful,
            FailedRequests         = failed,
            RequestsPerSecond      = records.Count / elapsed.TotalSeconds,
            AverageResponseTimeMs  = times.Average(),
            P50Ms                  = Percentile(times, 50),
            P95Ms                  = Percentile(times, 95),
            P99Ms                  = Percentile(times, 99),
            MinMs                  = times.First(),
            MaxMs                  = times.Last(),
            ErrorRate              = (double)failed / records.Count * 100,
        };
    }

    private static double Percentile(List<double> sorted, int percentile)
    {
        if (sorted.Count == 0) return 0;
        var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Count - 1)];
    }

    private static string LabelFromUrl(string url)
    {
        try { return new Uri(url).PathAndQuery; }
        catch { return url; }
    }
}

internal record RequestRecord(double ElapsedMs, bool Success, string? Error);
