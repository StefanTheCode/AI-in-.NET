# Semantic Search in .NET

Search that understands **meaning**, not just keywords. This console example embeds a list of blog-post titles with a local Ollama model, then ranks them by **cosine similarity** to your query — so "how do I make my database faster" surfaces the performance and EF Core posts even when the words don't match.

Part of the [AI in .NET Starter Kit](https://thecodeman.net/ai-in-dotnet-starter-kit) by [TheCodeMan](https://thecodeman.net).

---

## How it works

- **`Microsoft.Extensions.AI`** — the unified AI abstractions for .NET (`IEmbeddingGenerator`, `Embedding<T>`).
- **`OllamaSharp`** — connects to a local Ollama instance and generates embeddings with the `all-minilm` model.
- **`System.Numerics.Tensors`** — `TensorPrimitives` computes cosine similarity efficiently.

Each title is turned into a vector once; your query is turned into a vector at runtime; the closest vectors (by cosine similarity) are the best semantic matches.

---

## Prerequisites

- **.NET 10 SDK**
- **[Ollama](https://ollama.com)** running locally with the embedding model:
  ```bash
  ollama pull all-minilm
  ```
  The code connects to Ollama at `http://127.0.0.1:11434` — change the URI in `Program.cs` if yours differs.

---

## Run

```bash
cd SemanticSearch
dotnet run
```

Enter a query when prompted and watch the titles get ranked by semantic relevance. Try queries that don't share any words with the titles — that's where semantic search beats keyword search.

---

## Next step

Semantic search is the foundation for RAG — see the **RAG Basics** module in this kit to feed retrieved results to an LLM. And to build production AI features and agents in .NET, join the **[TheCodeMan AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)**.
