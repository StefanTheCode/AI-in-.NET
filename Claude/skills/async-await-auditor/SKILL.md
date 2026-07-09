---
name: async-await-auditor
description: Audit C#/.NET code for async/await anti-patterns and concurrency bugs. Use this skill whenever the user shares async C# code or asks about deadlocks, async void, .Result/.Wait(), blocking calls, ConfigureAwait, sync-over-async, fire-and-forget tasks, CancellationToken usage, or "why does my async code hang/deadlock". Always use this skill for async correctness/performance reviews instead of answering from memory; it applies a fixed checklist and rewrites the code.
---

# Async/Await Auditor

Find async/await anti-patterns and concurrency bugs in C# code, then return corrected code with a one-line reason for each fix.

## Input — point it at your code

You don't need pasted snippets. Work directly from whatever target the user gives:

- **A single file** — `path/to/Service.cs`
- **A folder** — `path/to/Features/`
- **A whole project or solution** — a `.csproj` / `.sln` or the repo root
- **A GitHub URL** — clone first, then scan: `git clone --depth 1 <url> /tmp/repo && cd /tmp/repo`

Find the async code yourself with Glob/Grep/Bash. Sweep for the anti-patterns first, e.g.:
```
grep -rn "\.Result\b\|\.Wait()\|GetAwaiter().GetResult()\|async void" --include=*.cs <target> | grep -vi "/obj/\|/bin/"
grep -rln "async Task" --include=*.cs <target>
```
On a large project, scan the files that matched the dangerous patterns first, then the broader async surface. Always report which files you scanned — and if it's clean, say so plainly instead of inventing issues. Group findings by file.

## When this runs

The user shares async C# (services, controllers, handlers, background workers) or a project/repo and wants it reviewed, or reports a hang/deadlock/threadpool-starvation symptom. They may give a path, folder, solution, or GitHub link — scan it yourself. Only ask for more if you need to know how an entry point is called (ASP.NET Core, console, library, UI).

## Workflow

1. **Read the call chain.** Async correctness is about the whole chain, not one method. Identify entry points and how far async flows.
2. **Run the checklist.** Flag every occurrence with severity (🔴 bug / 🟡 smell / 🟢 nit).
3. **Rewrite** the code preserving behavior; call out any behavior change.
4. **Explain each fix** in one line.

## The checklist

### 1. 🔴 Sync-over-async (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
Blocks a thread on an async call — deadlocks under a sync context, starves the thread pool otherwise.
- **Fix:** make the chain async and `await`. If truly at a sync boundary you can't change, isolate it and document why.

### 2. 🔴 `async void`
Exceptions can't be caught by the caller and crash the process; can't be awaited.
- **Detect:** `async void` methods (except genuine event handlers).
- **Fix:** return `Task`. For event handlers, wrap the body in try/catch and keep them thin.

### 3. 🔴 Fire-and-forget without handling
`_ = DoAsync();` or unawaited tasks swallow exceptions and may be killed when the request ends.
- **Fix:** await it, or hand off to a proper background mechanism (`IHostedService`, `Channel<T>`, a queue). Never just drop a task in request scope.

### 4. 🟡 Missing `CancellationToken`
Async APIs should accept and propagate a token.
- **Detect:** async methods without a `CancellationToken` parameter; tokens received but not passed down.
- **Fix:** thread the token through to the deepest async call (EF, HttpClient, etc.). In ASP.NET Core, accept `CancellationToken` in the action — it's bound to the request abort.

### 5. 🟡 `ConfigureAwait(false)` in library code
In libraries, not capturing the context avoids deadlocks and is slightly faster.
- **Detect:** library/shared code awaiting without `ConfigureAwait(false)`.
- **Fix:** add `ConfigureAwait(false)` in libraries. In ASP.NET Core app code it's unnecessary (no sync context) — don't litter it everywhere.

### 6. 🟡 Unnecessary `async`/`await` (elision vs. using)
- **Detect:** `return await SomeAsync();` as the only statement with no try/using around it.
- **Fix:** return the task directly (`return SomeAsync();`) — but NOT inside `using`/`try`, where eliding changes when disposal/exception handling happens. Know the difference.

### 7. 🟡 Sequential awaits that could be concurrent
Independent awaits run one after another.
- **Detect:** multiple independent `await`s in sequence.
- **Fix:** start them, then `await Task.WhenAll(...)`. Watch out: a single `DbContext` is NOT thread-safe — don't parallelize EF queries on one context.

### 8. 🟡 `Task.Run` to "make things async"
Wrapping CPU-trivial or already-async work in `Task.Run` wastes a thread pool thread, especially on the server.
- **Detect:** `Task.Run` around I/O or in ASP.NET request paths.
- **Fix:** call the async API directly. Reserve `Task.Run` for genuine CPU-bound work offloaded from a UI thread.

### 9. 🟡 `async` lambdas passed where `Action` is expected
Becomes `async void` silently (e.g. `list.ForEach(async x => ...)`).
- **Fix:** use a real loop with `await`, or `Task.WhenAll(list.Select(async x => ...))`.

### 10. 🟢 `ValueTask` misuse
- **Detect:** awaiting a `ValueTask` more than once, or storing it.
- **Fix:** await a `ValueTask` exactly once, immediately. Use `ValueTask` only on proven hot paths that often complete synchronously.

### 11. 🟡 Long-running / blocking work in `async` methods
`Thread.Sleep`, blocking locks, or heavy CPU inside async methods block the pool thread.
- **Fix:** `await Task.Delay(...)` instead of `Thread.Sleep`; `SemaphoreSlim.WaitAsync()` instead of `lock` when awaiting inside.

## Output format

```
## Summary
<headline issues and the deadlock/starvation risk, if any>

## Findings
🔴 [Sync-over-async] <where> — <why it's a bug> → <fix>
🟡 [Missing token] ... → ...

## Corrected code
<rewritten, ready to paste>

## Notes
<behavior changes, anything the caller must also update>
```

## Principles

- Async is a chain — a single blocking link can deadlock the whole thing.
- Don't cargo-cult `ConfigureAwait(false)` into ASP.NET Core app code; do use it in libraries.
- Never parallelize work on a shared `DbContext`.
- Prefer making the call chain async over hiding a blocking call.
