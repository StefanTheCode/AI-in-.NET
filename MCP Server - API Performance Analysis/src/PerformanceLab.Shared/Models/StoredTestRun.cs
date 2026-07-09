namespace PerformanceLab.Shared.Models;

public class StoredTestRun
{
    public string Id { get; init; } = "";
    public LoadTestResult Result { get; init; } = new();
    public AnalysisResult Analysis { get; init; } = new();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
