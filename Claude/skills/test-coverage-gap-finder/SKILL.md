---
name: test-coverage-gap-finder
description: Find the meaningful test-coverage gaps in a .NET project — untested public types, uncovered branches, and risky code without tests — and prioritize what to test next. Use this skill whenever the user wants to find coverage gaps, what to test next, untested code, low-coverage areas, or asks "what's not tested / where should I add tests / coverage report analysis". Always use this skill for coverage-gap analysis; it prioritizes by risk, not vanity percentage.
category: Testing
version: 1.0.0
---

# Test Coverage Gap Finder

Find where tests are actually missing and prioritize by risk — so effort goes to the code that matters, not chasing a coverage percentage.

## Input — point it at your project / coverage report
Works on a target: a **project** or **GitHub URL**, and optionally a coverage report (`coverage.cobertura.xml`, `lcov`). Map code vs tests:
```
grep -rln "public class\|public record" --include=*.cs <target> | grep -vi "Tests"
find <target> -iname "*Tests.cs" -o -iname "*.Tests.csproj"
# if a coverage file exists, parse it for low/zero-covered members
```
Generate coverage if asked: `dotnet test --collect:"XPlat Code Coverage"`.

## How it prioritizes (risk, not vanity %)
1. **Untested public types with real logic** — services, handlers, domain methods, validators. High priority.
2. **Uncovered branches in covered classes** — the `if/else`/switch arms tests skip (where bugs hide).
3. **Complex/critical code** — payment, auth, calculations, state transitions — weighted up even if partly covered.
4. **Deprioritize** — DTOs/POCOs, generated code, trivial getters: low value to test.

## Output
```
## Coverage gaps (ranked)
🔴 <Type> — no tests; contains <business logic> → suggest: <key cases>
🟡 <Type.Method> — branch `<condition>` uncovered → add case
🟢 (skip) <DTOs / generated / trivial>
## Suggested next 5 tests
1. ...  (hand off to xunit-test-generator)
```

## Principles
- **Coverage % is a vanity metric.** 100% of trivial code is worthless; 60% covering all the risky branches is great.
- Prioritize by **risk × complexity**, not line count.
- A gap in a payment/auth path outranks ten gaps in DTOs.
- Don't recommend tests for code that can't meaningfully fail.

## How to use it & best prompts
"What's not tested in this project", "where should I add tests next", "analyze my coverage report and prioritize", "find untested business logic". Hands off to `xunit-test-generator` and `test-data-builder-generator`.
