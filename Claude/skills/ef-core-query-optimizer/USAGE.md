# How to use — EF Core Query Optimizer

**What it is.** A skill that reviews your Entity Framework Core code and rewrites it for performance. It runs a fixed 10-point checklist (N+1, tracking, projections, cartesian explosion, client-side evaluation, pagination, bulk ops, indexing) and gives you back optimized code plus a one-line reason for every change.

**When to reach for it.** A slow endpoint, a query you suspect is doing too much, a code review where EF performance matters, or any time you mention N+1 / `AsNoTracking` / "why is this query slow".

**How to use it.** Point it at your code — no pasting needed. Give it a **file path**, a **folder**, a whole **project/solution**, or a **GitHub URL** (it clones and scans). It finds the EF queries itself. Example prompts:
- "Optimize the EF Core queries in `src/Orders/` for performance."
- "Review this repo's EF usage: https://github.com/me/myapi"
- "Scan the whole solution for N+1 and missing AsNoTracking."
- (still works) "Optimize this query: \<paste\>"

**How to get the best out of it.**
- **Give it the entities and relationships**, not just the query. Half the wins (projections, split queries) depend on knowing the shape of the data.
- **Tell it the symptom** ("slow on lists of 10k+", "times out on the dashboard"). It tunes the fix to the real problem instead of every micro-pattern.
- **Ask it to show the generated SQL** (`ToQueryString()`) when you want to verify a fix rather than trust it.
- For deep cases (compiled queries, pooling, split-query trade-offs) it pulls from its `references/ef-core-patterns.md` — ask explicitly if you want those.

**What it won't do.** It can't see your real database, so index suggestions come marked "verify" — confirm them against your schema and query plan. It won't change query results silently; if a fix alters behavior it tells you.
