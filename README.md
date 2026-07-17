# AI in .NET — Starter Kit

Everything you need to start building **real AI features in .NET 10** — three complete, runnable projects plus a set of Claude Code skills that make Claude write production-grade .NET for you.

This is the source bundle from **[thecodeman.net/ai-in-dotnet-starter-kit](https://thecodeman.net/ai-in-dotnet-starter-kit)**. Built and curated by [Stefan Đokić — TheCodeMan](https://thecodeman.net), Microsoft MVP.

---

## What's inside

| Folder | What it is |
|--------|-----------|
| **[Semantic Search AI Example](./Semantic%20Search%20AI%20Example)** | Search that understands *meaning*, not keywords. Local embeddings with Ollama + `Microsoft.Extensions.AI`. |
| **[RAG Basics](./RAG%20Basics)** | A minimal Retrieval-Augmented Generation pipeline — embed your text, store vectors in Postgres, retrieve and ground the LLM's answers in your data. |
| **[MCP Server - API Performance Analysis](./MCP%20Server%20-%20API%20Performance%20Analysis)** | A Model Context Protocol server that lets AI clients (Copilot, Claude, Cursor) diagnose .NET API performance in real time. |
| **[Claude](./Claude)** | A curated set of **Claude Code skills + an agent + a CLAUDE.md template** so Claude writes idiomatic .NET on your own projects. |

Each code module is a self-contained solution with its own README and a companion article on the blog.

---

## Prerequisites

- **.NET 10 SDK**
- **[Ollama](https://ollama.com)** running locally (for embeddings and local LLMs) — pull the models each module lists (e.g. `ollama pull all-minilm`)
- **PostgreSQL with pgvector** — the modules use [Neon Serverless Postgres](https://neon.tech), but any pgvector-enabled Postgres works
- For the MCP module: an MCP-compatible client (GitHub Copilot with MCP, or Claude Desktop)

> **Before you run anything:** open each module's `appsettings.json` and replace the connection strings / API keys with your own. Never commit real secrets.

---

## Quick start

1. Clone the repo (or unzip the download).
2. Pick a module and open its folder — start with **Semantic Search** if you're new to embeddings.
3. Read that module's README, set your `appsettings.json`, and `dotnet run`.
4. Install the **Claude** skills (see [`Claude/INSTALL.md`](./Claude/INSTALL.md)) and let Claude Code help you extend the code.

---

## Want to go further?

These three modules and five skills are the free starter. Inside the **[TheCodeMan AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)** you get the full arsenal — **44+ Claude skills, 7 specialist agents, and CLAUDE.md templates** — plus step-by-step lessons, live build-with-me sessions, and courses on building AI features and agents in .NET. It's where I teach how to actually use this in production.

📬 Weekly AI-in-.NET newsletter (20k+ .NET devs): **[thecodeman.net](https://thecodeman.net)** · ▶️ **[YouTube](https://www.youtube.com/@thecodeman_)**

---

## License

Free to use and adapt. See individual modules for details.

*Built by [Stefan Đokić](https://thecodeman.net) · [LinkedIn](https://www.linkedin.com/in/djokic-stefan/) · [X](https://x.com/TheCodeMan__)*
