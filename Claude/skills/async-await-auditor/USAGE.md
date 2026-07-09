# How to use — Async/Await Auditor

**What it is.** A skill that scans C#/.NET code for async/await anti-patterns and concurrency bugs, flags each one by severity (🔴 bug / 🟡 smell / 🟢 nit), and rewrites the code. Covers sync-over-async, `async void`, fire-and-forget, missing `CancellationToken`, `ConfigureAwait`, sequential-vs-concurrent awaits, `Task.Run` misuse, and more.

**When to reach for it.** A hang or deadlock, thread-pool starvation under load, a code review of async-heavy code, or before shipping a new background worker / handler.

**How to use it.** Point it at your code — a **file path**, a **folder**, a whole **project/solution**, or a **GitHub URL** (it clones and scans). It sweeps for the anti-patterns itself. Example prompts:
- "Audit `src/` for async/await problems."
- "Scan this repo for deadlocks and sync-over-async: https://github.com/me/myapi"
- "My API deadlocks under load — check the whole Services folder."
- (still works) "Audit this method: \<paste\>"

**How to get the best out of it.**
- **Include the caller, not just the method.** A blocking `.Result` three levels up is the real bug; the auditor needs the chain to find it.
- **Say where it runs.** `ConfigureAwait(false)` advice and deadlock risk differ between ASP.NET Core (no sync context) and libraries/UI.
- **Describe the symptom** if you have one (hangs, slow under load, exceptions vanishing) — it narrows the hunt.
- Ask it to **explain the deadlock mechanism** if you want to understand, not just patch.

**What it won't do.** It reasons from code, not a running profiler — for thread-pool starvation it points you at what to measure. It won't parallelize work on a shared `DbContext` (that's a bug it actively warns against).
