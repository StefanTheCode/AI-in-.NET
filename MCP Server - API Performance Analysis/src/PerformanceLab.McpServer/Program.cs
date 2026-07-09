using PerformanceLab.McpServer.Services;
using PerformanceLab.McpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

// ─── HttpClient for load testing ──────────────────────────────────────────────
builder.Services.AddHttpClient("loadtest", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    // Don't follow redirects automatically — record them as failed requests
    // so the load test reflects the real latency the client would see.
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect  = false,
    MaxConnectionsPerServer = 1000,
});

// ─── Application services ─────────────────────────────────────────────────────
builder.Services.AddSingleton<IResultStore, InMemoryResultStore>();
builder.Services.AddScoped<LoadTestRunner>();
builder.Services.AddScoped<ResultAnalyzer>();

// ─── MCP Server ───────────────────────────────────────────────────────────────
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// ─── REST API for Blazor Dashboard ────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddPolicy("dashboard", policy =>
        policy.WithOrigins("http://localhost:5300")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors("dashboard");

// MCP endpoint — used by GitHub Copilot, Claude, and any MCP client
app.MapMcp("/mcp");

// REST endpoints — used by the Blazor Dashboard
app.MapGet("/api/results", (IResultStore store) =>
    Results.Ok(store.GetAll()));

app.MapGet("/api/results/{id}", (string id, IResultStore store) =>
{
    var run = store.GetById(id);
    return run is null ? Results.NotFound() : Results.Ok(run);
});

app.MapDelete("/api/results", (IResultStore store) =>
{
    store.Clear();
    return Results.NoContent();
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

app.Run();
