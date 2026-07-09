# How to use — ASP.NET Core Security Auditor (agent)

**What it is.** An agent that audits an ASP.NET Core codebase against the OWASP Top 10 and .NET-specific risks — broken access control, secrets in source, injection, insecure config, CORS, mass assignment, weak auth, vulnerable dependencies. It produces a risk-ranked report where every finding shows the vulnerable code, the concrete attack it enables, and the fix.

**When to reach for it.** A pre-release security pass, a periodic review, after adding auth or new endpoints, or "is my API actually secure".

**How to use it.** Point it at the repo/solution. It explores the attack surface itself (Program.cs, controllers, endpoints, appsettings, csproj). Example prompts:
- "Run a security audit on this API."
- "Check my ASP.NET Core app against the OWASP Top 10 before we ship."
- "Are there any secrets or access-control gaps in this repo?"

**How to get the best out of it.**
- **Give it the full repo including config files** — a lot of findings live in `appsettings`, CORS setup, and `Program.cs`, not just controllers.
- **Let it run `dotnet list package --vulnerable`** (allow a restore) so the dependency-CVE check is real, not version-guessing.
- **Focus it** if you want — "just access control and auth" — to get a deeper pass on one area.
- **Act on the Quick Wins section first** — it's the highest-leverage subset.

**What it won't do.** It can't run the app, so some findings come marked "verify:" with how to confirm them. It's a code/config review, not a penetration test or DAST scan — treat it as the first line, not the only one. It won't invent vulnerabilities to pad the report.
