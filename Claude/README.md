# TheCodeMan .NET AI ToolKit

**44+ Claude skills and 7 specialist agents that make Claude Code an expert .NET developer** — running on *your* real code, not toy examples. EF Core, performance, architecture, testing, security, observability, DevOps. Built for modern **.NET 10 / C# 14**.

Curated by [Stefan Đokić — TheCodeMan](https://thecodeman.net), Microsoft MVP.

---

## ⚡ Install (one marketplace, two commands)

```shell
/plugin marketplace add StefanTheCode/dotnet-ai-toolkit
/plugin install dotnet-ai-toolkit@thecodeman-ai-toolkit
```

That's it. Skills trigger automatically when your request matches — e.g. *"optimize this EF query"*, *"scaffold a products endpoint"*, *"review this PR"* — and you invoke an agent by asking for what it does (*"audit the security of this API"*).

Update anytime:

```shell
/plugin marketplace update thecodeman-ai-toolkit
```

> Prefer manual install, or using these in Cowork? See **[INSTALL.md](INSTALL.md)**.

---

## What's a skill vs. an agent?

**Skill** — a focused capability Claude loads automatically when your request matches it. Great for "do this specific thing well" (optimize a query, generate tests, set up auth).

**Agent** — a specialist reviewer with its own system prompt that explores your codebase and produces a report. Great for "look at my whole repo and tell me what's wrong" (architecture review, security audit, code review).

---

## The library — 44 skills + 7 agents

| Category | Skills | Agents |
|----------|--------|--------|
| **Architecture** | clean-architecture-scaffolder, cqrs-mediatr-setup, result-pattern-scaffolder, minimal-api-endpoint-scaffolder, ddd-aggregate-generator, modular-monolith-generator, microservice-template-generator | dotnet-architecture-reviewer, legacy-modernization-assistant |
| **EF Core / Database** | ef-core-query-optimizer, ef-migration-reviewer, ef-index-advisor, sql-linq-converter, dbcontext-config-auditor, temporal-tables-setup, specification-pattern-generator, db-resiliency-setup | db-performance-auditor |
| **Performance** | async-await-auditor, benchmarkdotnet-setup, memory-allocation-analyzer, span-memory-refactor, caching-strategy-setup, gc-pressure-auditor, hotpath-profiler-assistant, stj-serialization-optimizer | — |
| **Observability** | opentelemetry-setup, serilog-logging-setup, healthchecks-setup, distributed-tracing-diagnostics, metrics-dashboard-generator, correlation-id-middleware | observability-gap-finder |
| **Testing** | xunit-test-generator, integration-test-setup, test-coverage-gap-finder, mutation-testing-setup, test-data-builder-generator, netarchtest-generator | — |
| **Quality** | — | dotnet-code-reviewer |
| **Security** | jwt-auth-setup, dependency-vuln-scanner, secrets-config-auditor, authorization-policy-generator | aspnetcore-security-auditor |
| **DevOps** | dockerfile-generator, cicd-pipeline-generator, nuget-dependency-analyzer, options-pattern-generator | dotnet-upgrade-agent |

Each skill and agent ships with a companion **USAGE** doc — what it is, when to use it, and example prompts.

---

## Want to go deeper?

This toolkit is the free, open layer. Inside the **[TheCodeMan AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)** I teach you, step by step, how to actually use AI in real .NET work — Claude Code workflows, building your own agents and skills, MCP servers in C#, and shipping AI features (RAG, agents) into your own apps. Live sessions and full courses, with members getting everything first.

📬 Weekly AI-in-.NET newsletter (20k+ .NET devs): [thecodeman.net](https://thecodeman.net) · ▶️ [YouTube](https://www.youtube.com/@thecodeman_)

---

## License

MIT — free to use and share. A ⭐ is always appreciated.

*Built by [Stefan Đokić](https://thecodeman.net) · [LinkedIn](https://www.linkedin.com/in/djokic-stefan/) · [X](https://x.com/TheCodeMan__)*
