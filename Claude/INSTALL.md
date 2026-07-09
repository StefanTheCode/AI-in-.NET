# Install & Use

## Option A — Claude Code plugin (easiest, recommended)

Install everything — all 44+ skills and 7 agents — with two commands:

```shell
/plugin marketplace add StefanTheCode/dotnet-ai-toolkit
/plugin install dotnet-ai-toolkit@thecodeman-ai-toolkit
```

Skills trigger automatically on matching requests; invoke an agent by asking for what it does (*"audit the security of this API"*). Pull updates anytime:

```shell
/plugin marketplace update thecodeman-ai-toolkit
```

Available in every project, no manual copying.

---

## Option B — Manual (single skill or agent)

**Skill** — copy the folder into a `skills/` directory Claude Code reads:

```bash
# project-level
mkdir -p .claude/skills && cp -r skills/ef-core-query-optimizer .claude/skills/
# or user-level (everywhere)
cp -r skills/ef-core-query-optimizer ~/.claude/skills/
```

**Agent** — copy the `.md` into the agents directory:

```bash
mkdir -p .claude/agents && cp agents/dotnet-architecture-reviewer.md .claude/agents/
```

Reopen the session. Skills trigger automatically; agents are invoked by asking for what they do.

---

## Verifying a skill works

1. Install it.
2. Start a fresh session and use a prompt from the skill's `USAGE.md` examples.
3. Confirm Claude applies the skill's checklist/format rather than a generic answer.

If a skill doesn't trigger, the fix is almost always the **description** in its `SKILL.md` frontmatter — make it specific about when to fire.
