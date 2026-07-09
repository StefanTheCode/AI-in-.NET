namespace PerformanceLab.Shared.Models;

public class LoadTestResult
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string Url { get; init; } = "";
    public string Label { get; init; } = "";
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public int DurationSeconds { get; init; }
    public int ConcurrentUsers { get; init; }
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public double RequestsPerSecond { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public double P50Ms { get; init; }
    public double P95Ms { get; init; }
    public double P99Ms { get; init; }
    public double MinMs { get; init; }
    public double MaxMs { get; init; }
    public double ErrorRate { get; init; }
}
