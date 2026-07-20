# AI Roadmap for .NET Developers — 2026

**The only guide you need to go from "AI-curious" to shipping real AI in .NET.**

No hype. No random tool lists. A clear path covering the two things every .NET developer now needs: **using AI to build faster** (Claude Code, agents, skills, MCP) and **building AI features into your own apps** (LLMs, embeddings, RAG, agents).

👉 **[Read the full roadmap →](docs/roadmap.md)**

Built by [Stefan Đokić — TheCodeMan](https://thecodeman.net), Microsoft MVP.

---

## The Path

```
Step 1: AI coding assistants — get faster today   → Week 1
Step 2: Skills, agents & custom workflows          → Week 2
Step 3: MCP — connect AI to your tools             → Week 3
Step 4: LLMs in .NET — your first AI feature       → Week 4
Step 5: Embeddings & semantic search               → Week 5
Step 6: RAG — ground the AI in your data           → Week 6
Step 7: AI agents in .NET                          → Week 7
Step 8: Production AI (evals, cost, guardrails)    → Week 8
```

Two tracks: **Use AI** (Steps 1–3) and **Build AI** (Steps 4–8). Each step builds on the last. Don't skip ahead.

---

## Companion code in this repo

The roadmap isn't just reading — three steps have full, runnable .NET projects right here to build from:

| Folder | Roadmap step | What it is |
|--------|--------------|-----------|
| **[Claude](./Claude)** | Step 2 | Claude Code **skills + an agent + a CLAUDE.md template** that make Claude write idiomatic .NET. |
| **[MCP Server - API Performance Analysis](./MCP%20Server%20-%20API%20Performance%20Analysis)** | Step 3 | A Model Context Protocol server in C# that lets AI clients (Copilot, Claude, Cursor) diagnose .NET API performance. |
| **[Semantic Search AI Example](./Semantic%20Search%20AI%20Example)** | Step 5 | Search by *meaning* — local embeddings with Ollama + `Microsoft.Extensions.AI`. |
| **[RAG Basics](./RAG%20Basics)** | Step 6 | A minimal RAG pipeline — embed, store vectors in Postgres, ground the LLM's answers in your data. |

Each project is a self-contained solution with its own README.

---

## Prerequisites (for the code)

- **.NET 10 SDK**
- **[Ollama](https://ollama.com)** running locally (embeddings + local LLMs) — pull the models each module lists (e.g. `ollama pull all-minilm`)
- **PostgreSQL with pgvector** — any pgvector-enabled Postgres works (e.g. [Neon](https://neon.tech))
- For the MCP module: an MCP-compatible client (GitHub Copilot with MCP, or Claude Desktop)

> **Before you run anything:** open each module's `appsettings.json` and set your **own** connection strings / API keys. Never commit real secrets.

---

## 🎥 A mini-course is coming

I'm recording this roadmap step by step as short video clips — a full mini-course walking through every stage with real .NET code. New clips drop inside the **[.NET AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)** (7-day free trial), where you also get the full skill set (44+ skills, 7 agents), the tools, and me answering your questions.

📬 Or follow along free: [weekly AI-in-.NET newsletter](https://thecodeman.net) (20k+ .NET devs) · ▶️ [YouTube](https://www.youtube.com/@thecodeman_)

---

## Who This Is For

- **.NET developers** who keep hearing about AI but don't know where it fits in real work
- **Backend engineers** who want to build AI features into their own apps
- **Teams** trying to actually use AI coding tools well, not just install them

---

## License

Free to use, share, and adapt. If it helps you, share it with a .NET dev who needs it. A ⭐ is appreciated.

*Built by [Stefan Đokić](https://thecodeman.net) · [LinkedIn](https://www.linkedin.com/in/djokic-stefan/) · [X](https://x.com/TheCodeMan__)*
