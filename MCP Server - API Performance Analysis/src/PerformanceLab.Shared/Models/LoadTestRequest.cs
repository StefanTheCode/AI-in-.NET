namespace PerformanceLab.Shared.Models;

public record LoadTestRequest
{
    public string Url { get; init; } = "";
    public int DurationSeconds { get; init; } = 10;
    public int ConcurrentUsers { get; init; } = 10;
    public string Method { get; init; } = "GET";
    public string? Body { get; init; }
}
