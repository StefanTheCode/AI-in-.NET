namespace PerformanceLab.Shared.Models;

public class AnalysisResult
{
    public string ResultId { get; init; } = "";
    public string Url { get; init; } = "";
    public string Severity { get; init; } = "Low";
    public List<string> Issues { get; init; } = [];
    public List<string> Recommendations { get; init; } = [];
    public List<string> Optimizations { get; init; } = [];
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
