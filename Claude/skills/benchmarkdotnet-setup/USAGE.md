# How to use — BenchmarkDotNet Setup

**What it is.** Writes correct BenchmarkDotNet benchmarks (avoiding the usual pitfalls) and interprets the results — time, ratio, and allocations.

**When to reach for it.** Comparing implementations, measuring a method, or making sense of benchmark output.

**How to use it.** Point it at the code (path or GitHub URL), or paste the methods/results. Example prompts:
- "Benchmark these two implementations."
- "Set up BenchmarkDotNet for this method."
- "Interpret these benchmark results for me."

**Get the best out of it.** Give it realistic input sizes. Keep `[MemoryDiagnoser]` on — allocations are often the real story. Confirm the path is actually hot first (`hotpath-profiler-assistant`) before optimizing.

**Won't do.** It won't profile a whole app — it's for focused micro-benchmarks. Run results in Release.
