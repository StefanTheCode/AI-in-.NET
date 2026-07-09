---
name: ef-core-query-optimizer
description: Analyze and optimize Entity Framework Core queries in .NET code. Use this skill whenever the user shares EF Core / LINQ-to-Entities code, a DbContext, a repository, or a slow query and wants it reviewed or sped up — or mentions N+1, lazy loading, AsNoTracking, slow EF queries, cartesian explosion, projections, or "why is my EF query slow". Always use this skill for EF Core performance questions instead of answering from memory; it applies a fixed checklist and rewrites the code.
---

# EF Core Query Optimizer

Find and fix the performance problems in Entity Framework Core queries, then return the optimized code with a short explanation of every change.

## Input — point it at your code

You don't need pasted snippets. Work directly from whatever target the user gives:

- **A single file** — `path/to/Query.cs`
- **A folder** — `path/to/Features/` (scan everything relevant under it)
- **A whole project or solution** — a `.csproj` / `.sln` or the repo root
- **A GitHub URL** — clone first, then scan: `git clone --depth 1 <url> /tmp/repo && cd /tmp/repo`

Discover the EF code yourself with Glob/Grep/Bash instead of asking for a paste. Locate candidates first, e.g.:
```
grep -rl "DbContext\|IQueryable" --include=*.cs <target> | grep -vi "/obj/\|/bin/"
grep -rn "\.Include(\|\.ToListAsync\|\.Where(\|\.Select(\|AsNoTracking" --include=*.cs <target>
```
On a large project, scan the matched files, **prioritize the worst offenders**, and don't read unrelated code. Always tell the user which files you scanned and group findings by file.

## When this runs

The user has shared EF Core code (queries, a `DbContext`, repositories, services) or a project/repo and wants it faster or reviewed. They may give a path, a folder, a solution, or a GitHub link — scan it yourself. Only ask for more if the target has no EF code or you need the entity/`DbContext` configuration to confirm a fix.

## Workflow

1. **Read the code and the data shape.** Identify the entities, relationships (1:1, 1:many, many:many), and what the query actually needs to return.
2. **Run the checklist below** against every query.
3. **Rewrite** the problematic queries. Keep behavior identical unless a bug is found (call out bugs separately).
4. **Explain each change** in one line — what was wrong, why it's slow, what the fix does.
5. **Flag what you cannot verify** (e.g. missing indexes need the DB schema; suggest the migration but mark it as "verify").

## The checklist

Apply every item. For each finding, state the problem, the impact, and the fix.

### 1. N+1 queries
The classic killer. A loop (or lazy navigation access) that fires one query per row.
- **Detect:** navigation property accessed inside a loop; `foreach` over entities then `.SomeNav`; lazy loading enabled.
- **Fix:** eager-load with `.Include()` / `.ThenInclude()`, or project into a DTO that pulls everything in one query.

### 2. Missing `AsNoTracking()`
Read-only queries should not pay the change-tracking cost.
- **Detect:** query results are returned/read but never modified and saved.
- **Fix:** add `.AsNoTracking()` (or `.AsNoTrackingWithIdentityResolution()` when the same entity appears multiple times in the graph).

### 3. Over-fetching (no projection)
Loading full entities when only a few columns are needed.
- **Detect:** `ToListAsync()` of full entities, then only a couple of properties used.
- **Fix:** `.Select(x => new XDto { ... })` to fetch only needed columns. Projection also removes the need for `Include` and avoids tracking.

### 4. Cartesian explosion
Multiple collection `Include`s in one query multiply rows.
- **Detect:** two or more collection navigations `Include`d in the same query.
- **Fix:** use `AsSplitQuery()`, or split into separate queries, or project. Note the trade-off (split query = multiple round-trips but no row multiplication).

### 5. Client-side evaluation
Work that silently runs in memory instead of SQL.
- **Detect:** custom C# methods, unmapped properties, or `.AsEnumerable()`/`.ToList()` *before* filtering/ordering.
- **Fix:** push filtering/ordering/paging into the query (before materialization); replace unmapped expressions with translatable ones.

### 6. Inefficient pagination
- **Detect:** `.Skip().Take()` on large offsets, or paging in memory.
- **Fix:** keyset pagination (`WHERE Id > @last ORDER BY Id`) for deep pages; ensure `OrderBy` is present and indexed.

### 7. Counting and existence
- **Detect:** `.Count() > 0`, `.ToList().Count`, `.Where(...).FirstOrDefault() != null`.
- **Fix:** `.AnyAsync(predicate)` for existence; `.CountAsync(predicate)` directly; avoid materializing.

### 8. Async all the way
- **Detect:** synchronous `ToList()`, `First()`, `SaveChanges()` in async code.
- **Fix:** `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()` with `CancellationToken`.

### 9. Bulk operations (EF Core 7+)
- **Detect:** load-then-modify-then-save loops for updates/deletes.
- **Fix:** `ExecuteUpdateAsync()` / `ExecuteDeleteAsync()` to do it in one SQL statement without loading entities.

### 10. Indexing & SQL inspection
- **Detect:** filters/joins/sorts on unindexed columns.
- **Fix:** suggest an index migration (`HasIndex`), but mark "verify against your schema". Recommend logging the generated SQL (`.LogTo(Console.WriteLine)` or `ToQueryString()`) to confirm.

## Output format

```
## Summary
<one paragraph: what was slow and the headline fixes>

## Findings
1. [N+1] <file/line> — <problem> → <fix>
2. [No projection] ... → ...

## Optimized code
<the rewritten query/method, ready to paste>

## Verify on your side
- <index suggestions, SQL to inspect, anything needing the real schema/data>
```

## Reference

For deeper patterns (compiled queries, `DbContext` pooling, split-query trade-offs, raw SQL escape hatches), see `references/ef-core-patterns.md`.

## Principles

- Never change query results silently. If a fix alters behavior, say so.
- Prefer projections over `Include` when the caller only reads data.
- Measure, don't guess: always recommend inspecting the generated SQL with `ToQueryString()`.
- Real-world over theoretical — optimize for the query that's actually slow, not every micro-pattern.
