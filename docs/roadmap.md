# The AI Roadmap for .NET Developers — Full Guide

This is the complete path. Work through it in order. Each step gives you: **what it is, why it matters, MUST vs OPTIONAL, what to build,** and **how you know you're ready to move on.**

Two tracks run through it:
- **Track A — Use AI (Steps 1–3):** ship .NET faster starting today.
- **Track B — Build AI (Steps 4–8):** put AI features into your own apps.

> This repo has the companion code for several steps — the `Semantic Search`, `RAG Basics`, and `MCP Server` projects, plus the `Claude/` skills. Use them as you go.

> The single mindset for this whole roadmap: **AI is a tool you drive, not magic you wait for.** You stay the engineer. It goes faster.

---

## Step 1 — AI coding assistants: get faster today
**Week 1 · Track A**

The fastest ROI in your whole career is learning to code with an AI assistant *well*. Most developers install one, get mediocre output, and blame the tool. The difference is context and prompting.

**MUST learn**
- Pick one assistant and go deep: **Claude Code**, **GitHub Copilot**, or **Cursor**.
- The **`CLAUDE.md` / rules file**: a file at your repo root that tells the AI your stack, architecture, and conventions. This is the single biggest quality lever — it stops the AI writing generic C# (controllers, repositories, AutoMapper) and makes it write *yours*.
- Prompting for real work: describe the *outcome* and constraints, not the steps. Ask for a plan before edits. Tell it what to **never** do.

**OPTIONAL**
- Trying all three assistants side by side (later, not now).
- Editor-specific power-user tricks.

**Build it**
- Add a `CLAUDE.md` to a real project — grab the ready template in [`Claude/templates/`](../Claude/templates) or generate one with the free CLAUDE.md Generator.
- Take one real task from your backlog and finish it fully with the assistant.

**Common mistakes**
- No context file → generic output → you blame the AI.
- Accepting code you don't understand. Review everything.

**✅ You're ready when:** the AI matches your project's style on the first try, and you know why.

---

## Step 2 — Skills, agents & custom workflows
**Week 2 · Track A**

Out of the box the assistant is general. You make it a *.NET specialist* by giving it reusable capabilities.

**MUST learn**
- **Skills** — focused, reusable instructions the AI loads automatically when your request matches ("optimize this EF query", "scaffold an endpoint", "write integration tests").
- **Agents / subagents** — specialists that explore your whole codebase and produce a report (architecture, security, code review).
- **Slash commands & hooks** (Claude Code) — repeatable workflows and automation.

**OPTIONAL**
- Writing your own skills from scratch (do it once you've *used* enough).
- Multi-agent orchestration.

**Build it**
- Install the skills in [`Claude/`](../Claude) (this repo) and run 5 of them on a real repo.
- Write **one** custom skill for a task you repeat weekly.

**Common mistakes**
- Collecting 50 skills you never use. Start with the 5 that match your daily work.

**✅ You're ready when:** you have a small set of skills you actually reach for, and you've written one of your own.

---

## Step 3 — MCP: connect AI to your tools
**Week 3 · Track A → B bridge**

**MCP (Model Context Protocol)** is how AI clients talk to external tools and data in a standard way — the bridge from "AI that writes code" to "AI that can *do things* in your systems."

**MUST learn**
- What MCP is and why it exists (one protocol, many clients: Claude, Copilot, Cursor).
- **Using** existing MCP servers from your client.
- **Building** a simple MCP server in **C#** with the official `ModelContextProtocol` SDK.

**OPTIONAL**
- HTTP/remote MCP transport and hosting (start with local stdio).
- Auth and advanced tool schemas.

**Build it**
- Study and run the [`MCP Server - API Performance Analysis`](../MCP%20Server%20-%20API%20Performance%20Analysis) project in this repo, then add one tool of your own. (Or scaffold from scratch with the free MCP Server Generator.)
- Connect it to your AI client and call your tool by name.

**Common mistakes**
- Logging to stdout on a stdio server (it corrupts the protocol — log to stderr).
- Exposing write/dangerous operations without guardrails.

**✅ You're ready when:** your AI client can call a tool you wrote in C#.

---

## Step 4 — LLMs in .NET: your first AI feature
**Week 4 · Track B**

Now you build AI *into* an app. Start with a single, clean LLM call behind an abstraction.

**MUST learn**
- **`Microsoft.Extensions.AI`** and the **`IChatClient`** abstraction — swap providers without rewriting your app.
- Calling a provider: **Azure OpenAI**, **OpenAI**, or **Ollama** (local, free) for dev.
- **Streaming** responses and **structured output** (JSON you can deserialize).
- Keeping the API key in config, never in code.

**OPTIONAL**
- Fine-tuning (you almost never need it — skip).
- Multiple providers in one app.

**Build it**
- Add one AI feature to an existing .NET app behind `IChatClient` (summarize, classify, or draft), with streaming.

**Common mistakes**
- Hardcoding one provider's SDK everywhere. Abstract behind `IChatClient` from day one.
- No timeout / cancellation on the call.

**✅ You're ready when:** you can add an LLM call to any .NET app, swap the provider by config, and stream the result.

---

## Step 5 — Embeddings & semantic search
**Week 5 · Track B**

Embeddings turn text into vectors so you can search by **meaning**, not keywords — the foundation of everything "AI over your data."

**MUST learn**
- What an embedding is; generating them (`IEmbeddingGenerator`, Ollama `all-minilm`, or a hosted model).
- **Cosine similarity** and why it ranks results.
- Storing/querying vectors: **pgvector** (Postgres) or **Qdrant**.

**OPTIONAL**
- Advanced indexes (HNSW tuning), hybrid search — later.

**Build it**
- Run and extend the [`Semantic Search AI Example`](../Semantic%20Search%20AI%20Example) project in this repo.

**Common mistakes**
- Mismatched embedding models between indexing and querying (must be the same).
- Storing full entities instead of just what you need.

**✅ You're ready when:** you can embed text, store it, and retrieve the most *relevant* results for a query.

---

## Step 6 — RAG: ground the AI in your data
**Week 6 · Track B**

**Retrieval-Augmented Generation** = retrieve the relevant chunks, then let the LLM answer *from your data* instead of guessing. The most in-demand applied-AI skill right now.

**MUST learn**
- The pipeline: **chunk → embed → store → retrieve top-k → feed to the LLM**.
- The 3 decisions that matter: **chunking strategy, embedding model, and top-k**.
- Grounding the prompt so answers cite your data.

**OPTIONAL**
- Re-ranking, query rewriting, agentic retrieval — level 2.

**Build it**
- Run and extend the [`RAG Basics`](../RAG%20Basics) project in this repo into a document Q&A.

**Common mistakes**
- Chunks too big or too small. Start ~500 tokens with overlap, then tune.
- No source attribution — users won't trust ungrounded answers.

**✅ You're ready when:** your app answers questions from *your* documents, not the model's training data.

---

## Step 7 — AI agents in .NET
**Week 7 · Track B**

An **agent** decides *which tools to call* to accomplish a goal — reasoning + action, not just a single response.

**MUST learn**
- **Tool calling / function calling**: give the LLM your C# methods as tools.
- An agent framework: **Semantic Kernel** or the **Microsoft Agent Framework** — planning, tools, memory.
- **Guardrails:** agents should *suggest and act within limits*, not run wild. Human-in-the-loop for anything destructive.

**OPTIONAL**
- Multi-agent systems and complex orchestration — advanced.

**Build it**
- A focused agent that does one real job (triages errors, reviews a PR, answers support questions) using your MCP tools + RAG.

**Common mistakes**
- Over-scoping. A narrow agent that does one thing well beats a "do everything" agent that's unreliable.
- No limits on what tools can do.

**✅ You're ready when:** you have an agent that picks tools and completes a real, bounded task safely.

---

## Step 8 — Production AI: evals, cost, guardrails
**Week 8 · Track B**

Shipping AI is different from shipping CRUD. This step separates a demo from a product.

**MUST learn**
- **Evaluation:** how do you know a change made it *better*? Basic evals / test cases for AI outputs.
- **Cost & tokens:** measure usage, cache where you can, pick the right model per task.
- **Guardrails & security:** **prompt injection** is the new SQL injection — never trust retrieved/user text as instructions. Validate tool inputs. Keep secrets/PII out of prompts and logs.
- **Observability:** trace LLM calls, latency, and failures (OpenTelemetry).

**OPTIONAL**
- Self-hosting models, advanced eval frameworks — when scale demands it.

**Build it**
- Add evals, a cost log, and prompt-injection guarding to your RAG/agent app from Steps 6–7.

**✅ You're ready when:** you can measure quality and cost, and explain how your AI feature resists prompt injection.

---

## The 8-Week Plan

| Week | Focus | Ship |
|---|---|---|
| 1 | AI assistant + `CLAUDE.md` | One backlog task done fully with AI |
| 2 | Skills & agents | 5 skills in use + 1 custom skill |
| 3 | MCP in C# | An MCP server with one real tool |
| 4 | LLM in .NET | One AI feature behind `IChatClient` |
| 5 | Embeddings | A semantic search |
| 6 | RAG | Document Q&A grounded in your data |
| 7 | Agents | A bounded agent that uses tools |
| 8 | Production | Evals + cost + guardrails on it |

Two months, eight things shipped. That's a portfolio.

---

## What NOT to Learn (Right Now)

Skipping these saves you months:

- ❌ **Training / fine-tuning your own models** — you almost never need it. Use hosted/local models + RAG.
- ❌ **The math of transformers** — interesting, not required to ship.
- ❌ **Every new framework of the week** — the ecosystem churns; the *concepts* (LLM, embeddings, RAG, agents, MCP) are stable. Learn those.
- ❌ **Prompt-engineering "hacks"** — good context + clear intent beats clever tricks.
- ❌ **Chasing benchmarks** — pick a capable model, ship, measure on *your* task.

---

## Portfolio Projects

Build these to prove it (three are already in this repo to start from):

1. **Semantic Search API** — search content by meaning (Steps 4–5). → `Semantic Search AI Example/`
2. **Document Q&A (RAG)** — grounded answers with sources (Step 6). → `RAG Basics/`
3. **MCP Server in C#** — expose operations as tools any AI client can use (Step 3). → `MCP Server - API Performance Analysis/`
4. **A focused agent** — triage, review, or support automation (Steps 7–8).

Each one is a LinkedIn post, a portfolio piece, and a real skill.

---

## Where This Is Going

AI won't replace good .NET developers. But developers who can **use AI to build faster** and **build AI into their products** are already pulling ahead. This roadmap is the path. Now go build.

---

## Go deeper (free)

- 🧩 [.NET AI ToolKit skills](https://github.com/StefanTheCode/dotnet-ai-toolkit) · 📄 CLAUDE.md Generator · 🔌 MCP Server Generator

**A step-by-step mini-course** (video clips for every stage) is being recorded inside the **[.NET AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)** — 7-day free trial, plus the full skill set and me answering questions.

📬 [Weekly newsletter](https://thecodeman.net) · ▶️ [YouTube](https://www.youtube.com/@thecodeman_)

*By [Stefan Đokić — TheCodeMan](https://thecodeman.net), Microsoft MVP.*
