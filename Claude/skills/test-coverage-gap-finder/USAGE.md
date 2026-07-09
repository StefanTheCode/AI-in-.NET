# How to use — Test Coverage Gap Finder

**What it is.** Finds meaningful test gaps (untested public types, uncovered branches, risky code) and prioritizes what to test next by risk — not by chasing a coverage percentage.

**When to reach for it.** Deciding where to add tests, or making sense of a coverage report.

**How to use it.** Point it at your project (path or GitHub URL), optionally with a coverage file. Example prompts:
- "What's not tested in this project?"
- "Analyze my coverage report and prioritize."
- "Find untested business logic and tell me what to write."

**Get the best out of it.** Give it a coverage report if you have one (`dotnet test --collect:"XPlat Code Coverage"`) for branch-level gaps. Then hand the top items to `xunit-test-generator`.

**Won't do.** It won't push for 100% coverage — it deprioritizes DTOs/trivial code on purpose.
