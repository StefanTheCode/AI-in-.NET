# 5 Claude Code Skills for .NET — Starter Set

A curated set of **5 Claude Code skills** (plus a security agent and a CLAUDE.md template) that make Claude write idiomatic, production-grade **.NET 10 / C# 14** on your own projects — instead of generic C#.

Curated by [Stefan Đokić — TheCodeMan](https://thecodeman.net), Microsoft MVP.

---

## ⚡ Install (one marketplace, two commands)

```shell
/plugin marketplace add StefanTheCode/dotnet-claude-starter
/plugin install dotnet-claude-starter@thecodeman-claude-starter
```

That's it — the 5 skills and the security agent install into Claude Code, in every project. Skills trigger automatically when your request matches; the agent runs when you ask for what it does.

Update anytime:

```shell
/plugin marketplace update thecodeman-claude-starter
```

> Prefer a manual copy instead? See **[INSTALL.md](INSTALL.md)**.

---

## What's inside

### 🧩 Skills (5)

| Skill | What it does |
|-------|--------------|
| **ef-core-query-optimizer** | Finds and fixes slow EF Core queries — N+1, missing projections, missing `AsNoTracking`, cartesian explosion, compiled queries for hot paths. |
| **async-await-auditor** | Audits async/await for deadlocks, `async void`, `.Result`/`.Wait()`, sync-over-async, fire-and-forget, and `CancellationToken` misuse — and rewrites it. |
| **clean-architecture-scaffolder** | Scaffolds a .NET solution in Clean Architecture or Vertical Slice — layers, project references, CQRS/MediatR, and a working sample feature. |
| **benchmarkdotnet-setup** | Sets up BenchmarkDotNet properly (benchmark classes, memory diagnostics, baselines) and explains the ns/op + allocation numbers. |
| **test-coverage-gap-finder** | Finds the *meaningful* test gaps — untested public types, uncovered branches, risky code — and prioritizes by risk, not vanity percentage. |

### 🤖 Bonus agent (1)

| Agent | What it does |
|-------|--------------|
| **aspnetcore-security-auditor** | Audits an ASP.NET Core codebase against the OWASP Top 10 and .NET-specific risks (auth gaps, injection, secrets in source, CORS, mass assignment, vulnerable dependencies) and produces a ranked report with fixes. |

### 📄 Bonus template (1)

| Template | What it does |
|----------|--------------|
| **dotnet-CLAUDE-md-template.md** | A drop-in `CLAUDE.md` for your repo root. It tells Claude your stack, structure, conventions, and the patterns to *never* suggest — so it writes your .NET, not someone else's, from the first prompt. |

---

## Skill vs. agent

**Skill** — a focused capability Claude loads automatically when your request matches it. It does the thing.

**Agent** — a specialist that explores your codebase on its own and produces a report. You invoke it by asking for what it does.

---

## How to use

Install, then just talk to Claude Code naturally:

```
> This EF query is slow, optimize it
> Review this async code for deadlocks
> Scaffold a clean architecture solution
> Set up BenchmarkDotNet to compare these two methods
> What's not tested in this project?
> Audit the security of this API      ← runs the security agent
```

For the CLAUDE.md template, copy it to your repo root as `CLAUDE.md` and fill in the blanks (instructions are at the top of the file).

---

## Want the full toolkit?

This is 5 skills. The full **[TheCodeMan AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)** has the whole set — **44+ skills and 7 specialist agents** across EF Core, performance, architecture, testing, security, observability, and DevOps — plus MCP servers in C#, building real AI features in .NET, live build-with-me sessions, and full courses. It's where I actually teach you how to use AI in real .NET work, not just hand you files.

**Open on a 7-day free trial** → [join the community](https://www.skool.com/thecodeman-ai-toolkit-9723)

📬 Weekly AI-in-.NET newsletter (20k+ .NET devs): [thecodeman.net](https://thecodeman.net) · ▶️ [YouTube](https://www.youtube.com/@thecodeman_)

---

*Built by [Stefan Đokić](https://thecodeman.net) · [LinkedIn](https://www.linkedin.com/in/djokic-stefan/) · [X](https://x.com/TheCodeMan__)*
