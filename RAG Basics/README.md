# RAG Basics in .NET

A minimal **Retrieval-Augmented Generation (RAG)** pipeline in ASP.NET Core — so an LLM answers from *your* data instead of guessing. You add text, it gets embedded and stored as vectors in Postgres; at query time the most relevant chunks are retrieved and fed to a local Ollama model to ground the answer.

Part of the [AI in .NET Starter Kit](https://thecodeman.net/ai-in-dotnet-starter-kit) by [TheCodeMan](https://thecodeman.net).

---

## How it works

```
Your text ──▶ Ollama embeddings ──▶ store vectors in Postgres (pgvector)
                                            │
Question ──▶ embed question ──▶ retrieve top matches ──▶ Ollama LLM ──▶ grounded answer
```

Key pieces:

- `EmbeddingGenerator/` — `OllamaEmbeddingGenerator` turns text into vectors via a local Ollama model.
- `Repository/` — `TextContext` + `TextRepository` store and query embeddings in Postgres.
- `Services/RagService.cs` — ties retrieval + generation together.
- `Program.cs` — minimal API endpoints (see `RagBasics.http` for sample requests).

---

## Prerequisites

- **.NET 10 SDK**
- **[Ollama](https://ollama.com)** running locally — pull the models used in the code, e.g.:
  ```bash
  ollama pull all-minilm      # embeddings
  ollama pull llama3           # or whichever LLM the code references
  ```
- **PostgreSQL with the pgvector extension** (the sample uses [Neon Serverless Postgres](https://neon.tech), but any pgvector Postgres works)

---

## Setup

1. Open `RagBasics/appsettings.json` and set your **own** Postgres connection string:

   ```json
   "ConnectionStrings": {
     "PostgreSQL": "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
   }
   ```

   > ⚠️ Do not commit real credentials. Use your own database and keep secrets out of source control (User Secrets / environment variables).

2. Make sure the `vector` extension is enabled on your database:

   ```sql
   CREATE EXTENSION IF NOT EXISTS vector;
   ```

---

## Run

```bash
cd RagBasics
dotnet run
```

Then use `RagBasics.http` (or Swagger) to add some text and ask a question. You'll see the answer grounded in the text you ingested.

---

## Want to go further?

Building production RAG, agents, and MCP servers in .NET is exactly what I teach in the **[TheCodeMan AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)** — with the full skill set, lessons, and courses.
