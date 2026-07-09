namespace PerformanceLab.Tests;

public class LoadTestRunnerTests
{
    // ─── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Builds an IHttpClientFactory that always returns responses with the given
    /// status code after the given delay — without making real network calls.
    /// </summary>
    private static IHttpClientFactory BuildFactory(
        HttpStatusCode status = HttpStatusCode.OK,
        int delayMs = 5)
    {
        var handler = new DelayedResponseHandler(status, delayMs);
        var client  = new HttpClient(handler);
        var factory = new SingleClientFactory(client);
        return factory;
    }

    // ─── Tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_Returns_NonZeroRps()
    {
        var runner = new LoadTestRunner(BuildFactory(), NullLogger<LoadTestRunner>.Instance);
        var req    = new LoadTestRequest { Url = "http://localhost/fast", DurationSeconds = 2, ConcurrentUsers = 5 };

        var result = await runner.RunAsync(req, CancellationToken.None);

        result.RequestsPerSecond.Should().BeGreaterThan(0);
        result.TotalRequests.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunAsync_AllSuccessful_WhenServerReturns200()
    {
        var runner = new LoadTestRunner(BuildFactory(HttpStatusCode.OK), NullLogger<LoadTestRunner>.Instance);
        var req    = new LoadTestRequest { Url = "http://localhost/ok", DurationSeconds = 2, ConcurrentUsers = 3 };

        var result = await runner.RunAsync(req, CancellationToken.None);

        result.FailedRequests.Should().Be(0);
        result.ErrorRate.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_RecordsFailures_WhenServerReturns500()
    {
        var runner = new LoadTestRunner(BuildFactory(HttpStatusCode.InternalServerError), NullLogger<LoadTestRunner>.Instance);
        var req    = new LoadTestRequest { Url = "http://localhost/broken", DurationSeconds = 2, ConcurrentUsers = 3 };

        var result = await runner.RunAsync(req, CancellationToken.None);

        result.FailedRequests.Should().BeGreaterThan(0);
        result.ErrorRate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunAsync_PopulatesPercentiles()
    {
        var runner = new LoadTestRunner(BuildFactory(), NullLogger<LoadTestRunner>.Instance);
        var req    = new LoadTestRequest { Url = "http://localhost/fast", DurationSeconds = 2, ConcurrentUsers = 5 };

        var result = await runner.RunAsync(req, CancellationToken.None);

        result.P50Ms.Should().BeGreaterThanOrEqualTo(result.MinMs);
        result.P95Ms.Should().BeGreaterThanOrEqualTo(result.P50Ms);
        result.P99Ms.Should().BeGreaterThanOrEqualTo(result.P95Ms);
        result.MaxMs.Should().BeGreaterThanOrEqualTo(result.P99Ms);
    }

    [Fact]
    public async Task RunAsync_IdIs8Chars()
    {
        var runner = new LoadTestRunner(BuildFactory(), NullLogger<LoadTestRunner>.Instance);
        var req    = new LoadTestRequest { Url = "http://localhost/fast", DurationSeconds = 1, ConcurrentUsers = 1 };

        var result = await runner.RunAsync(req, CancellationToken.None);

        result.Id.Should().HaveLength(8);
    }

    [Fact]
    public async Task RunAsync_Respects_CancellationToken()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        var runner    = new LoadTestRunner(BuildFactory(delayMs: 50), NullLogger<LoadTestRunner>.Instance);
        var req       = new LoadTestRequest { Url = "http://localhost/slow", DurationSeconds = 60, ConcurrentUsers = 2 };

        // Should complete quickly (cancelled) not after 60 seconds
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await runner.RunAsync(req, cts.Token);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        result.TotalRequests.Should().BeGreaterThan(0);
    }

    // ─── Fake HTTP infrastructure ──────────────────────────────────────────

    private sealed class DelayedResponseHandler(HttpStatusCode status, int delayMs)
        : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(delayMs, cancellationToken);
            return new HttpResponseMessage(status);
        }
    }

    private sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }
}
