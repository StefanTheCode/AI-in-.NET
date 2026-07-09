# How to use — Clean Architecture Scaffolder

**What it is.** A skill that scaffolds a .NET solution in either **Clean Architecture** (layered: Domain/Application/Infrastructure/API) or **Vertical Slice Architecture** (feature-first). It gives you the `dotnet` CLI commands, the project references wired correctly, a working CQRS sample feature, and an architecture guardrail test.

**When to reach for it.** Starting a new .NET project, restructuring a messy one, adding a new feature/slice in a consistent way, or settling "how should I structure this".

**How to use it.** For an existing project, point it at the repo (a path or GitHub URL) and it detects your house style and scaffolds a new feature that matches. For a new solution, tell it the **style** (or ask it to recommend one), **project name**, **.NET version**, and whether you want **CQRS/MediatR**. Example prompts:
- "Scaffold a Clean Architecture solution called Acme, .NET 9, with MediatR."
- "I want Vertical Slice — set up the structure and show me a CreateOrder slice."
- "Should this CRUD app be Clean or Vertical Slice? Then scaffold it."

**How to get the best out of it.**
- **Let it recommend the style** if you're unsure — describe the team size and project, and it picks (it actively warns against over-engineering a CRUD app into four layers).
- **Ask for one real feature**, not just folders. The working CQRS sample is the template you'll copy for everything else.
- **Keep the guardrail test.** It generates a NetArchTest that fails the build if someone violates the dependency rule — that's what stops the structure from rotting.
- **Use it incrementally** — come back to add each new slice/feature so they stay consistent.

**What it won't do.** It won't build your whole domain — it gives the skeleton and one exemplar feature. It won't force a heavy architecture on a simple app; pushing back on over-engineering is part of the skill.
