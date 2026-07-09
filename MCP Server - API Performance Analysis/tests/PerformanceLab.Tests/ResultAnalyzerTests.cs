namespace PerformanceLab.Tests;

public class ResultAnalyzerTests
{
    private readonly ResultAnalyzer _sut = new();

    // ─── Analyze — severity grading ────────────────────────────────────────

    [Fact]
    public void Analyze_FastEndpoint_ReturnsSeverityLow()
    {
        var result = BuildResult(avgMs: 50, p95: 80, p99: 100, p50: 45, maxMs: 120, rps: 500, errorRate: 0);

        var analysis = _sut.Analyze(result);

        analysis.Severity.Should().Be("Low");
        analysis.Issues.Should().Contain(i => i.Contains("No significant"));
    }

    [Fact]
    public void Analyze_HighAverageLatency_ReturnsSeverityHigh()
    {
        var result = BuildResult(avgMs: 1500, p95: 2500, p99: 3000, p50: 1200, maxMs: 4000, rps: 5, errorRate: 0);

        var analysis = _sut.Analyze(result);

        analysis.Severity.Should().BeOneOf("High", "Critical");
        analysis.Issues.Should().Contain(i => i.Contains("blocking") || i.Contains("latency") || i.Contains("tail"));
    }

    [Fact]
    public void Analyze_CriticalErrorRate_ReturnsSeverityCritical()
    {
        var result = BuildResult(avgMs: 200, p95: 400, p99: 600, p50: 180, maxMs: 800, rps: 30, errorRate: 15);

        var analysis = _sut.Analyze(result);

        analysis.Severity.Should().Be("Critical");
        analysis.Issues.Should().Contain(i => i.ToLower().Contains("error"));
    }

    [Fact]
    public void Analyze_ThreadPoolStarvationSignature_DetectsRatio()
    {
        // p99/p50 > 10 is the starvation signature
        var result = BuildResult(avgMs: 800, p95: 3000, p99: 8000, p50: 100, maxMs: 9000, rps: 3, errorRate: 0);

        var analysis = _sut.Analyze(result);

        analysis.Issues.Should().Contain(i => i.Contains("starvation") || i.Contains("Thread.Sleep") || i.Contains("variance"));
        analysis.Recommendations.Should().Contain(r => r.Contains("Task.Delay") || r.Contains("await") || r.Contains("async"));
    }

    [Fact]
    public void Analyze_GcPausePattern_DetectsSpikeInMax()
    {
        // Max > p99 * 3 signals GC pause
        var result = BuildResult(avgMs: 100, p95: 200, p99: 300, p50: 90, maxMs: 1500, rps: 200, errorRate: 0);

        var analysis = _sut.Analyze(result);

        analysis.Issues.Should().Contain(i => i.Contains("spike") || i.Contains("GC") || i.Contains("pause"));
    }

    [Fact]
    public void Analyze_PopulatesRecommendationsForHighSeverity()
    {
        var result = BuildResult(avgMs: 2000, p95: 4000, p99: 6000, p50: 1800, maxMs: 7000, rps: 2, errorRate: 0);

        var analysis = _sut.Analyze(result);

        analysis.Recommendations.Should().NotBeEmpty();
    }

    // ─── Compare ───────────────────────────────────────────────────────────

    [Fact]
    public void Compare_CandidateFaster_ReportsImprovement()
    {
        var baseline  = BuildResult(avgMs: 1000, p95: 2000, p99: 3000, p50: 900, maxMs: 4000, rps: 10,  errorRate: 0);
        var candidate = BuildResult(avgMs:  100, p95:  200, p99:  300, p50:  90, maxMs:  400, rps: 100, errorRate: 0);

        var comparison = _sut.Compare(baseline, candidate);

        comparison.IsCandidateFaster.Should().BeTrue();
        comparison.RpsChangePercent.Should().BeGreaterThan(50);
        comparison.AvgLatencyChangePercent.Should().BeLessThan(-50);
        comparison.Winner.Should().Be("Candidate");
    }

    [Fact]
    public void Compare_CandidateSlower_ReportsRegression()
    {
        var baseline  = BuildResult(avgMs:  100, p95:  200, p99:  300, p50:  90, maxMs:  400, rps: 100, errorRate: 0);
        var candidate = BuildResult(avgMs: 1000, p95: 2000, p99: 3000, p50: 900, maxMs: 4000, rps: 10,  errorRate: 0);

        var comparison = _sut.Compare(baseline, candidate);

        comparison.IsCandidateFaster.Should().BeFalse();
        comparison.Winner.Should().Be("Baseline");
    }

    [Fact]
    public void Compare_SummaryIsNotEmpty()
    {
        var baseline  = BuildResult(avgMs: 500, p95: 800, p99: 1000, p50: 450, maxMs: 1500, rps: 50, errorRate: 0);
        var candidate = BuildResult(avgMs: 200, p95: 300, p99:  400, p50: 180, maxMs:  600, rps: 90, errorRate: 0);

        var comparison = _sut.Compare(baseline, candidate);

        comparison.Summary.Should().NotBeNullOrWhiteSpace();
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private static LoadTestResult BuildResult(
        double avgMs, double p95, double p99, double p50,
        double maxMs, double rps, double errorRate, double minMs = 10) =>
        new()
        {
            Id                    = Guid.NewGuid().ToString("N")[..8],
            Url                   = "http://localhost:5100/test",
            Label                 = "test",
            Timestamp             = DateTimeOffset.UtcNow,
            DurationSeconds       = 10,
            ConcurrentUsers       = 10,
            TotalRequests         = (int)(rps * 10),
            SuccessfulRequests    = (int)(rps * 10 * (1 - errorRate / 100)),
            FailedRequests        = (int)(rps * 10 * errorRate / 100),
            RequestsPerSecond     = rps,
            AverageResponseTimeMs = avgMs,
            P50Ms                 = p50,
            P95Ms                 = p95,
            P99Ms                 = p99,
            MinMs                 = minMs,
            MaxMs                 = maxMs,
            ErrorRate             = errorRate,
        };
}
