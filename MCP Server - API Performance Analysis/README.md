# Performance Lab — ASP.NET Core Performance MCP Server

An educational demo project showing how to build an MCP (Model Context Protocol) server that uses AI to diagnose .NET API performance problems in real time.

## What it does

You connect GitHub Copilot (or Claude, or any MCP-compatible client) to this project, and then ask it in natural language to:

- Run load tests against any HTTP endpoint
- Compare two endpoints side by side
- Detect ThreadPool starvation, GC pressure, or high error rates
- Suggest concrete .NET fixes based on the data
- Generate markdown performance reports

The project includes a sample API with intentionally broken endpoints so you can see all the problems in action.

## Projects

| Project | Port | Description |
|---------|------|-------------|
| `PerformanceLab.Api` | 5100 | Sample ASP.NET Core API with educational (intentionally flawed) endpoints |
| `PerformanceLab.McpServer` | 5200 | MCP server + REST API — the brain |
| `PerformanceLab.Dashboard` | 5300 | Blazor Server dashboard for visualising results |
| `PerformanceLab.Shared` | — | Shared models (no external dependencies) |
| `PerformanceLab.Tests` | — | xUnit tests for core logic |

## Quick start

### Prerequisites
- .NET 10 SDK
- GitHub Copilot with MCP support, or Claude Desktop

### 1. Clone and build

```bash
git clone <repo-url>
cd PerformanceLab
dotnet build
```

### 2. Start all three services

Open three terminals:

```bash
# Terminal 1 — Sample API
dotnet run --project src/PerformanceLab.Api

# Terminal 2 — MCP Server
dotnet run --project src/PerformanceLab.McpServer

# Terminal 3 — Dashboard
dotnet run --project src/PerformanceLab.Dashboard
```

### 3. Connect your AI client

#### GitHub Copilot (VS Code)

Add this to your workspace `.vscode/mcp.json` (or user-level settings):

```json
{
  "servers": {
    "performance-lab": {
      "type": "http",
      "url": "http://localhost:5200/mcp"
    }
  }
}
```

Reload the window. You should see "performance-lab" in the Copilot MCP server list.

#### Claude Desktop

Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "performance-lab": {
      "url": "http://localhost:5200/mcp"
    }
  }
}
```

### 4. Run your first test

In GitHub Copilot chat (Agent mode):

```
Run a load test against http://localhost:5100/fast for 10 seconds with 20 users
```

Then compare the slow endpoint with the fast one:

```
Compare http://localhost:5100/slow-thread-sleep with http://localhost:5100/slow-task-delay
```

Get a diagnosis:

```
Analyze the results for run <ID from the output above>
```

## Sample API Endpoints

| Endpoint | What it demonstrates |
|----------|---------------------|
| `GET /fast` | Baseline — returns immediately |
| `GET /slow-thread-sleep?ms=500` | ❌ Blocks a ThreadPool thread with `Thread.Sleep` |
| `GET /slow-task-delay?ms=500` | ✅ Correct async pattern with `await Task.Delay` |
| `GET /memory-heavy` | Allocates ~1MB per request — GC pressure |
| `GET /cpu-heavy?iterations=50000` | CPU-bound work (SHA256 hash loop) |
| `GET /database-simulation` | N+1 anti-pattern — 5 sequential awaits (~250ms) |
| `GET /optimized-version` | Fixed with `Task.WhenAll` (~50ms) |

## MCP Tools Available

| Tool | Description |
|------|-------------|
| `run_load_test` | Run a load test, get result ID |
| `compare_endpoints` | Test two URLs and compare side by side |
| `analyze_results` | Full diagnosis for a result ID |
| `compare_before_after` | Compare two stored result IDs |
| `detect_slow_responses` | Check for slow latency patterns |
| `detect_threadpool_starvation` | p99/p50 ratio analysis |
| `detect_memory_pressure` | GC pause spike detection |
| `generate_report` | Full markdown report for one or more IDs |
| `suggest_optimizations` | Pattern-based .NET fix suggestions |
| `list_results` | List all stored test run IDs |

## Running Tests

```bash
dotnet test
```

Expected: 15 tests, all passing.

## Dashboard

Open [http://localhost:5300](http://localhost:5300) after starting the dashboard.

The dashboard calls the McpServer REST API (`/api/results`) to display stored results and allows side-by-side comparison of any two test runs.

## Architecture

```
AI Client (Copilot / Claude)
        │ MCP Protocol (HTTP SSE)
        ▼
PerformanceLab.McpServer (:5200)
  ├─ /mcp          ← MCP tools for AI clients
  ├─ /api/results  ← REST API for dashboard
  │
  ├─ LoadTestRunner    — fires concurrent HTTP requests
  ├─ ResultAnalyzer    — rule-based diagnosis
  └─ InMemoryResultStore — holds results in memory
        │ HTTP
        ▼
PerformanceLab.Api (:5100)
  └─ Sample endpoints (fast, slow, memory-heavy, etc.)

PerformanceLab.Dashboard (:5300)
  └─ Blazor Server — reads /api/results from McpServer
```

## Example AI Prompts

```
Run a load test against http://localhost:5100/slow-thread-sleep for 15 seconds with 20 users

Compare http://localhost:5100/database-simulation with http://localhost:5100/optimized-version

Detect threadpool starvation for result <ID>

Do you see any GC pressure symptoms in result <ID>?

Generate a full performance report for <ID1>,<ID2>

What .NET optimizations would you suggest for threadpool issues?
```
