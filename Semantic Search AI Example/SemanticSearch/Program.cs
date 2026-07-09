using System.Numerics.Tensors; // Provides TensorPrimitives for efficient mathematical operations like cosine similarity
using Microsoft.Extensions.AI;      // Unified AI abstractions for .NET (IEmbeddingGenerator, Embedding<T>, etc.)
using OllamaSharp;                  // Ollama client that implements IEmbeddingGenerator for local model inference

// Initialize the embedding generator using OllamaSharp's OllamaApiClient.
// It connects to a locally running Ollama instance and uses the "all-minilm" model
// to convert text into numerical vector representations (embeddings).
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaApiClient(new Uri("http://127.0.0.1:11434"), "all-minilm");

// Define blog post titles as candidates for similarity search.
var blogPostTitles = new[]
{
    "Debug and Test Multi-Environment Postgres Db in .NET with Aspire + Neon",
    "Simplifying Integration with the Adapter Pattern",
    "Getting Started with OpenTelemetry in .NET",
    "Saga Orchestration Pattern",
    ".NET 9 - New LINQ Methods",
    "HybridCache in ASP.NET Core - .NET 9",
    "Chain Responsibility Pattern",
    "Exploring C# 13",
    "Feature Flags in .NET 8 with Azure Feature Management",
    "Securing Secrets in .NET 8 with Azure Key Vault",
    "LINQ Performance Optimization Tips & Tricks",
    "Using Singleton in Multithreading in .NET",
    "How to create .NET Custom Guard Clause",
    "How to implement CQRS without MediatR",
    "4 Entity Framework Tips to improve performances",
    "REPR Pattern - For C# developers",
    "Refit - The .NET Rest API you should know about",
    "6 ways to eleveate your 'clean' code",
    "Deep dive into Source Generators",
    "3 Tips to Elevate your Swagger UI",
    "Memory Caching in .NET",
    "Solving HttpClient Authentication with Delegating Handlers",
    "Strategy Design Pattern will help you refactor code",
    "How to implement API Key Authentication",
    "Live loading appsettings.json configuration file",
    "Retry Failed API calls with Polly",
    "How and why I create my own mapper (avoid Automapper)?",
    "The ServiceCollection Extension Pattern",
    "3 things you should know about Strings",
    "Api Gateways - The secure bridge for exposing your api",
    "5 cool features in C# 12",
    "Allow specific users to access your API - Part 2",
    "Allow specific users to access your API - Part 1",
    "Response Compression in ASP.NET",
    "API Gateway with Ocelot",
    "Health Checks in .NET 8",
    "MediatR Pipeline Behavior",
    "Getting Started with PLINQ",
    "Get Started with GraphQL in .NET",
    "Better Error Handling with Result<T> object",
    "Background Tasks in .NET 8",
    "Pre-Optimized EF Query Techniques 5 Steps to Success",
    "Improve EF Core Performance with Compiled Queries",
    "How do I implement a workflow using a .NET workflow engine?",
    "What is and why do you need API Versioning?",
    "Compile-time logging source generation for highly performant logging",
    "Background tasks and how to use them. Stale Cache example in C#",
    "Unlock the Power of High-Performance Web Applications with gRPC",
    "Using CORS in your applications",
    "HashIDs. What are they, and why should we use them?",
    "Benchmarking in .NET Step by step",
    "How do I create Middleware? And what are the alternatives?",
    "How to use MediatR Notifications email",
    "How to send email in 5min with FluentEmail?",
    "Real-Time applications with SignalR",
    "Use Architecture Tests in your projects",
    "Structured Logging with Serilog",
    "SOLID Principles in .NET",
    "Jobs in .NET with Hangfire",
    "Make your .NET application secure",
    "How to put localhost online in Visual Studio?",
    "How to implement a Rate Limiter in C#",
    "Clean Code - Best Practices",
    "SAGA Implementation in C#",
    "GitHub Webhook with C#",
    "How to use ChatGPT in C# application?",
    "4 methods to handle Nullable Reference in .NET"
};

Console.WriteLine("Generating embeddings for blog post titles...");

// Generate embeddings for all blog post titles in a single batch call.
// GenerateAndZipAsync returns a list of (Value, Embedding) pairs,
// linking each original title with its corresponding embedding vector.
var candidateEmbeddings = await embeddingGenerator.GenerateAndZipAsync(blogPostTitles);
Console.WriteLine("Embeddings generated successfully.");

while (true)
{
    Console.WriteLine("\nEnter your query (or press Enter to exit):");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    // Generate a single embedding vector for the user's search query.
    var userEmbedding = await embeddingGenerator.GenerateAsync(userInput);

    // Compare the user's query embedding against every blog post embedding
    // using cosine similarity — a value between -1 and 1 where 1 means identical direction.
    // TensorPrimitives.CosineSimilarity uses SIMD-accelerated operations for performance.
    var topMatches = candidateEmbeddings
        .Select(candidate => new
        {
            Text = candidate.Value,
            Similarity = TensorPrimitives.CosineSimilarity(candidate.Embedding.Vector.Span, userEmbedding.Vector.Span)
        })
        .OrderByDescending(match => match.Similarity)
        .Take(3); // Return only the top 3 most semantically similar results

    Console.WriteLine("\nTop matching blog post titles:");
    foreach (var match in topMatches)
    {
        Console.WriteLine($"Similarity: {match.Similarity:F4} - {match.Text}");
    }
}