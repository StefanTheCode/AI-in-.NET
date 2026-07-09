using System.Collections.Concurrent;
using PerformanceLab.Shared.Models;

namespace PerformanceLab.McpServer.Services;

/// <summary>
/// In-memory store for load test results.
/// Structured so the underlying store can be swapped for SQLite/PostgreSQL later
/// by replacing this implementation and keeping the same interface.
/// </summary>
public interface IResultStore
{
    void Add(LoadTestResult result, AnalysisResult analysis);
    IReadOnlyList<StoredTestRun> GetAll();
    StoredTestRun? GetById(string id);
    void Clear();
}

public sealed class InMemoryResultStore : IResultStore
{
    private readonly ConcurrentDictionary<string, StoredTestRun> _store = new();

    public void Add(LoadTestResult result, AnalysisResult analysis)
    {
        var run = new StoredTestRun
        {
            Id        = result.Id,
            Result    = result,
            Analysis  = analysis,
            Timestamp = DateTimeOffset.UtcNow,
        };

        _store[result.Id] = run;
    }

    public IReadOnlyList<StoredTestRun> GetAll() =>
        [.. _store.Values.OrderByDescending(r => r.Timestamp)];

    public StoredTestRun? GetById(string id) =>
        _store.TryGetValue(id, out var run) ? run : null;

    public void Clear() => _store.Clear();
}
