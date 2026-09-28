**🏗️ October Build Challenge: 12 FREE C# projects to learn AI in .NET**

"If AI writes the code now, what's even the point of building side projects?"

I get this question a lot. My answer: the point changed, it didn't disappear.

AI gives you syntax in seconds. It doesn't tell you why the query is slow, why the auth is leaking, or why the "working" solution falls apart with 50 users. You only learn that by building things and breaking them.

So I put together **12 projects, 100% free**: full source code plus a step-by-step build guide for each one. Together they take you through the whole AI-in-.NET path:

- **Starter & Intermediate (01–06):** using AI to build faster (Claude Code, Copilot, Codex) *without* shipping code you don't understand
- **Advanced (07–09):** putting AI *inside* your app: chatbots with streaming, RAG over your own docs, structured output you can trust
- **Expert (10–12):** building things AI plugs into: your own MCP server, an agent that calls tools in a loop, and an AI app in production with evals, token costs, and OpenTelemetry

No paywall on the projects. You only pay with your time.

So in October we build.

**How it works:**

1. Pick ONE project at your level
2. Open its BUILD-GUIDE on GitHub and build it from an empty repo, with AI
3. Keep an AI-MISSES.md file. Every time AI gets something wrong, write it down.
4. Tick every box in the Definition of done
5. Post your repo here by October 31

👉 All 12 projects + guides: https://github.com/StefanTheCode/AI-in-.NET/blob/main/CHALLENGE.md

Every guide has step-by-step prompts, "Check yourself" questions you answer WITHOUT AI, the places where AI usually gets it wrong in that exact project, and experiments to break it on purpose.

A few examples of what you'll run into:

- In the Job Runner, a single HttpClient timeout can silently kill your whole worker
- In RAG, "What's the capital of France?" still pulls 3 chunks from your docs
- In the Invoice Parser, the model sometimes "fixes" wrong totals, so your validation passes a bad invoice
- In the Code Review Agent, one comment inside the file can tell the agent to ignore all bugs

One rule for the whole month: **let AI speed you up, never let it think for you.**

**Reply below with:**

Level:
Project:
My goal by Oct 31:

Then drop your progress in this thread every week. If you get stuck, and on Advanced and Expert you will, post the project number, what you tried, and the error. That's what this group is for.

Which level are you at right now? 👇
