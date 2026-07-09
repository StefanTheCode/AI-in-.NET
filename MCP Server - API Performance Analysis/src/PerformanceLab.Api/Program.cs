using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
//  /fast  — returns immediately. No delay, no blocking. Baseline for comparison.
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/fast", () =>
    Results.Ok(new
    {
        message  = "pong",
        endpoint = "/fast",
        note     = "Returned immediately. This is your performance baseline.",
        ts       = DateTimeOffset.UtcNow
    }));

// ─────────────────────────────────────────────────────────────────────────────
//  /slow-thread-sleep  — INTENTIONALLY BAD
//  Uses Thread.Sleep which blocks a ThreadPool thread for the entire wait.
//  Under load this causes ThreadPool starvation: new requests queue up because
//  all threads are sleeping, not free to handle incoming work.
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/slow-thread-sleep", (int ms = 500) =>
{
    Thread.Sleep(ms); // ← blocks the thread. NEVER do this in a real API.

    return Results.Ok(new
    {
        message  = "done",
        endpoint = "/slow-thread-sleep",
        blockedMs = ms,
        warning   = "Thread.Sleep blocks a ThreadPool thread. Under load, this causes starvation.",
        fix       = "Use await Task.Delay(ms) instead."
    });
});

// ─────────────────────────────────────────────────────────────────────────────
//  /slow-task-delay  — CORRECT async pattern
//  Uses await Task.Delay which releases the thread back to the pool while
//  waiting. The same thread can serve other requests during the delay.
//  This is why async matters under load — it's not just about "looking async".
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/slow-task-delay", async (int ms = 500) =>
{
    await Task.Delay(ms); // ← frees the thread. Correct pattern.

    return Results.Ok(new
    {
        message   = "done",
        endpoint  = "/slow-task-delay",
        waitedMs  = ms,
        note      = "Task.Delay frees the thread while waiting. The ThreadPool is happy.",
        advantage = "Under load, async waiting scales far better than Thread.Sleep."
    });
});

// ─────────────────────────────────────────────────────────────────────────────
//  /memory-heavy  — INTENTIONALLY BAD
//  Allocates large short-lived objects per request, putting GC pressure on the
//  application. In a real scenario this could be: reading full blobs into memory,
//  building huge string concatenations, or using ToList() on large queries.
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/memory-heavy", () =>
{
    // Allocates 100 arrays × 10 KB = ~1 MB per request, all immediately eligible for GC.
    var allocations = new List<byte[]>(100);
    for (var i = 0; i < 100; i++)
    {
        allocations.Add(new byte[10_000]);
        // Simulate "work" to prevent JIT from optimizing the allocations away
        allocations[i][0] = (byte)(i % 256);
    }

    var totalBytes = allocations.Count * 10_000;

    return Results.Ok(new
    {
        message         = "done",
        endpoint        = "/memory-heavy",
        allocatedObjects = allocations.Count,
        approximateBytes = totalBytes,
        warning         = "Allocating large short-lived objects causes frequent GC pauses.",
        fix             = "Use ArrayPool<T>, Span<T>, or stream data instead of loading it all into memory."
    });
});

// ─────────────────────────────────────────────────────────────────────────────
//  /cpu-heavy  — CPU pressure simulation
//  Simulates a CPU-bound operation (e.g. report generation, image processing,
//  cryptographic work, tight loops). CPU-bound work should be offloaded to
//  background threads and never done on ASP.NET Core's request thread under load.
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/cpu-heavy", (int iterations = 50_000) =>
{
    using var sha = SHA256.Create();
    var data = Encoding.UTF8.GetBytes("thecodeman.net performance lab cpu benchmark");
    var hash = data;

    for (var i = 0; i < iterations; i++)
    {
        hash = sha.ComputeHash(hash);
    }

    return Results.Ok(new
    {
        message    = "done",
        endpoint   = "/cpu-heavy",
        iterations,
        finalHash  = Convert.ToHexString(hash)[..16],
        warning    = "CPU-bound work blocks the request thread and starves other requests.",
        fix        = "Use Task.Run for CPU-bound work, or better: move it to a background service / queue."
    });
});

// ─────────────────────────────────────────────────────────────────────────────
//  /database-simulation  — N+1 query anti-pattern
//  Simulates 5 sequential database calls (e.g. loading a parent record and then
//  querying each child one by one). Even though each call is async, doing them
//  sequentially means 5 × 50ms = 250ms total.
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/database-simulation", async () =>
{
    var records = new List<string>();

    // Sequential async calls — correct use of await, but wrong architecture
    for (var i = 0; i < 5; i++)
    {
        await Task.Delay(50); // each "query" costs 50ms
        records.Add($"Record-{i}");
    }

    return Results.Ok(new
    {
        message        = "done",
        endpoint       = "/database-simulation",
        recordsFetched = records.Count,
        pattern        = "sequential-async (N+1)",
        estimatedMs    = 250,
        note           = "5 sequential async calls. Correct async pattern, but wrong architecture.",
        fix            = "Use Task.WhenAll to run independent queries in parallel. See /optimized-version."
    });
});

// ─────────────────────────────────────────────────────────────────────────────
//  /optimized-version  — Same 5 queries, but in parallel
//  Demonstrates Task.WhenAll: all 5 "queries" fire at the same time.
//  Total time ≈ 50ms instead of 250ms. Same correctness, 5× faster.
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/optimized-version", async () =>
{
    var tasks = Enumerable.Range(0, 5)
        .Select(async i =>
        {
            await Task.Delay(50); // each "query" costs 50ms
            return $"Record-{i}";
        })
        .ToList();

    var records = await Task.WhenAll(tasks); // all 5 fire simultaneously

    return Results.Ok(new
    {
        message        = "done",
        endpoint       = "/optimized-version",
        recordsFetched = records.Length,
        pattern        = "parallel-async (Task.WhenAll)",
        estimatedMs    = 50,
        note           = "Same 5 queries running in parallel with Task.WhenAll.",
        improvement    = "5× faster than sequential version. Same correctness guarantees."
    });
});

// ─────────────────────────────────────────────────────────────────────────────
//  /health  — simple health check for the load tester
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "healthy", ts = DateTimeOffset.UtcNow }));

// ─────────────────────────────────────────────────────────────────────────────
//  /endpoints  — lists all available endpoints with descriptions
// ─────────────────────────────────────────────────────────────────────────────
app.MapGet("/endpoints", () => Results.Ok(new
{
    available = new[]
    {
        new { path = "/fast",                description = "Returns immediately. Use as baseline." },
        new { path = "/slow-thread-sleep",   description = "Blocks thread with Thread.Sleep. ?ms=500" },
        new { path = "/slow-task-delay",     description = "Non-blocking async wait. ?ms=500" },
        new { path = "/memory-heavy",        description = "Allocates ~1MB of short-lived objects per request." },
        new { path = "/cpu-heavy",           description = "SHA256 hash loop. ?iterations=50000" },
        new { path = "/database-simulation", description = "5 sequential async calls (N+1 pattern)." },
        new { path = "/optimized-version",   description = "Same 5 calls in parallel with Task.WhenAll." },
    }
}));

app.Run();
