using System.Net.Http.Json;
using PerformanceLab.Shared.Models;

namespace PerformanceLab.Dashboard.Services;

public class McpApiClient(HttpClient http)
{
    public async Task<List<StoredTestRun>> GetAllResultsAsync(CancellationToken ct = default)
    {
        var results = await http.GetFromJsonAsync<List<StoredTestRun>>("/api/results", ct);
        return results ?? [];
    }

    public async Task<StoredTestRun?> GetResultAsync(string id, CancellationToken ct = default)
    {
        try
        {
            return await http.GetFromJsonAsync<StoredTestRun>($"/api/results/{id}", ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task ClearResultsAsync(CancellationToken ct = default)
    {
        await http.DeleteAsync("/api/results", ct);
    }
}
