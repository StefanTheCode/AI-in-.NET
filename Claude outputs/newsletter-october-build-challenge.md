# Newsletter: October Build Challenge

## Subject line (pick one)

1. 12 free C# projects to learn AI in .NET (and a challenge)
2. If AI writes the code, why build side projects?
3. Pick one project. Finish it by October 31.

**Preview text:** Starter to Expert: RAG, agents, MCP, production AI. Full source code + step-by-step guides, all free.

---

## Body

Hey friend,

Last week someone asked me:

*"If AI writes the code now, what's even the point of building side projects?"*

Fair question. My take: the point changed, it didn't disappear.

Before, a side project was mostly about learning syntax. Now AI spits out syntax in seconds. What it doesn't give you is the feel for whether that code is any good. Why the query is slow. Why the auth is leaking. Why the "working" solution falls apart with 50 users.

That's the skill that pays now. And you only get it by building stuff and breaking it.

So I built **12 C# projects for the AI era**. They're **100% free**: full source code, plus a step-by-step build guide for each one.

### The path

**01 · Starter:** Expense Tracker CLI, To-Do REST API, Weather Dashboard
*LINQ, EF Core, async. Use AI, but read every line it writes.*

**02 · Intermediate:** URL Shortener, Blog API with Auth, Background Job Runner
*Caching, rate limiting, JWT, retries. This is where AI starts being wrong in small, sneaky ways.*

**03 · Advanced:** AI Support Chatbot, Docs Q&A with RAG, Smart Invoice Parser
*AI inside your app: Microsoft.Extensions.AI, embeddings, vector search, structured output.*

**04 · Expert:** Your Own MCP Server, AI Code Review Agent, AI App in Production
*Things other tools plug into: MCP, the agent loop, evals, token costs, OpenTelemetry.*

### What's in every guide

- Step-by-step prompts to use with Claude Code, Copilot, or Codex
- "Check yourself" questions you answer *without* AI
- The exact places where AI usually gets it wrong in that project
- Experiments to break it on purpose
- A Definition of done, so you know when you're actually finished

A few things you'll run into:

- In the Job Runner, a single HttpClient timeout can silently kill your whole worker.
- In RAG, "What's the capital of France?" still pulls 3 chunks from your docs.
- In the Invoice Parser, the model sometimes "fixes" wrong totals, and your validation passes a bad invoice.
- In the Code Review Agent, one comment inside a file can tell the agent to ignore every bug.

**[→ Get all 12 projects on GitHub](https://github.com/StefanTheCode/AI-in-.NET/blob/main/CHALLENGE.md)**

### The October Build Challenge

Here's the deal: **pick ONE project and finish it by October 31.**

Not twelve. One. Finished, and understood line by line.

We're running the challenge inside my community, **[AI for .NET Developers](https://www.skool.com/ai-for-dotnet-developers/about)**:

- Weekly check-ins, so you actually finish
- A place to post your progress and your repo
- Someone to ask when you get stuck (and on Advanced and Expert, you will: Ollama won't connect, the agent won't call tools, the MCP client silently disconnects...)

There's a 7-day free trial, so you can join, pick your project, and see if it's for you.

**[→ Join the challenge in AI for .NET Developers](https://www.skool.com/ai-for-dotnet-developers/about)**

One rule for all 12: **let AI speed you up, never let it think for you.**

Hit reply and tell me: which level are you at right now? I read every answer.

Stefan

---

*P.S. Even if you don't join the challenge, grab the repo and do one project this month. Save this email, block 2 hours this weekend, and start with Step 1.*
