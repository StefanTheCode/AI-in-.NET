namespace PerformanceLab.Shared.Models;

public class CompareResult
{
    public LoadTestResult Baseline { get; init; } = new();
    public LoadTestResult Candidate { get; init; } = new();
    public double RpsChangePercent { get; init; }
    public double AvgLatencyChangePercent { get; init; }
    public double P95ChangePercent { get; init; }
    public double P99ChangePercent { get; init; }
    public double ErrorRateAbsoluteDiff { get; init; }
    public bool IsCandidateFaster { get; init; }
    public string Winner { get; init; } = "";
    public string Summary { get; init; } = "";
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
