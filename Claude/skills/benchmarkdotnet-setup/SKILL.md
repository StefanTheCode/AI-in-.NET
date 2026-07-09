---
name: benchmarkdotnet-setup
description: Set up BenchmarkDotNet for a .NET project and interpret the results — proper benchmark classes, memory diagnostics, baselines, and reading ns/op + allocations. Use this skill whenever the user wants to benchmark code, measure performance, compare two implementations, set up BenchmarkDotNet, or asks "which is faster / how do I benchmark this / interpret these benchmark results". Always use this skill for micro-benchmarking requests; it writes correct benchmarks and explains the numbers.
category: Performance
version: 1.0.0
---

# BenchmarkDotNet Setup

Write correct BenchmarkDotNet benchmarks (no common pitfalls) and read the results properly — so "which is faster" is answered with data, not vibes.

## Input — point it at your code
Works on a target: a **file**, **folder**, **project**, or **GitHub URL**. Identify the methods/implementations to compare; if the user pasted results, go straight to interpretation.

## Project + benchmark class
```
dotnet add package BenchmarkDotNet
```
```csharp
[MemoryDiagnoser]                     // allocations matter as much as time
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class StringJoinBenchmarks
{
    private readonly string[] _items = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToArray();

    [Benchmark(Baseline = true)]
    public string StringConcat() { var s = ""; foreach (var i in _items) s += i; return s; }

    [Benchmark]
    public string StringBuilder_() { var sb = new StringBuilder(); foreach (var i in _items) sb.Append(i); return sb.ToString(); }

    [Benchmark]
    public string StringJoin() => string.Join("", _items);
}
// Program.cs:  BenchmarkRunner.Run<StringJoinBenchmarks>();
```
Run in **Release**: `dotnet run -c Release`.

## Pitfalls the skill prevents
- Benchmarking in Debug, or without `BenchmarkRunner` (JIT/optimizations differ).
- Dead-code elimination — **return** the result so the work isn't optimized away.
- Setup work inside the `[Benchmark]` method — move it to `[GlobalSetup]`.
- Comparing without a `[Baseline]` — you need a reference point.
- Tiny inputs that measure noise, not the algorithm.

## Reading the summary
- **Mean (ns/us/ms)** — central tendency; check **StdDev/Error** for noise.
- **Ratio** — vs baseline (the headline comparison).
- **Gen0/1/2 + Allocated** — allocations and GC pressure; often the real story, not raw time.
- A faster method that allocates 10× more may lose under real load — weigh both.

## Principles
- Measure the realistic input size and shape, not a toy.
- Allocations are a first-class result — keep `[MemoryDiagnoser]` on.
- One variable at a time; keep benchmarks isolated and deterministic.
- Don't micro-optimize a path that isn't hot — confirm it matters first (`hotpath-profiler-assistant`).

## How to use it & best prompts
"Benchmark these two implementations", "is StringBuilder faster here", "set up BenchmarkDotNet for this method", "interpret these benchmark results".
