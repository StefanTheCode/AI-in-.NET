# Install & Use

These files are a starter set you copy into Claude Code. Two folders matter: `skills/` and `agents/`, plus the `templates/` file.

## Install the skills

Copy the skill folders into a `skills/` directory Claude Code reads:

```bash
# user-level — available in every project (recommended)
mkdir -p ~/.claude/skills
cp -r skills/* ~/.claude/skills/

# or project-level — just this project
mkdir -p .claude/skills
cp -r skills/* .claude/skills/
```

## Install the agent

```bash
mkdir -p ~/.claude/agents
cp agents/aspnetcore-security-auditor.md ~/.claude/agents/
```

## Add the CLAUDE.md template

Copy `templates/dotnet-CLAUDE-md-template.md` to the **root of your .NET project**, rename it to `CLAUDE.md`, and fill in the blanks (instructions are at the top of the file). Claude Code loads it automatically at the start of every session.

---

## Use

Reopen your Claude Code session, then just describe what you want:

```
> This EF query is slow, optimize it
> Review this async code for deadlocks
> Scaffold a clean architecture solution
> Set up BenchmarkDotNet to compare these two methods
> What's not tested in this project?
> Audit the security of this API      ← runs the security agent
```

Skills trigger automatically when your request matches; the agent runs when you ask for what it does.

---

## Verifying a skill works

1. Install it (above) and reopen the session.
2. Use a prompt from the skill's `USAGE.md` examples.
3. Confirm Claude applies the skill's checklist/format rather than a generic answer.

If a skill doesn't trigger, the fix is almost always the **description** in its `SKILL.md` frontmatter — make it specific about when to fire.

---

## Want the full toolkit?

This is 5 skills + 1 agent. The full set — **44+ skills, 7 agents, and CLAUDE.md templates** — lives inside the **[TheCodeMan AI ToolKit community](https://www.skool.com/thecodeman-ai-toolkit-9723)**, with lessons, live sessions, and courses on top.
