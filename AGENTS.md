# Hospital Game

Godot 4.5 C# game. Currently a hello-world scaffold with a clickable square.

## Commands

```bash
# Run game (orchestrator handles this)
/Applications/Godot_mono.app/Contents/MacOS/Godot --path .

# Build C#
/usr/local/share/dotnet/dotnet build

# Format code
/usr/local/share/dotnet/dotnet format
```

## Structure

```
scenes/           → .tscn scene files
scripts/          → .cs C# scripts
assets/game-icons → 4,200+ SVG icons (CC-BY, game-icons.net)
.research/        → Godot 4.5.1 source (gitignored)
```

## Prototype Visuals

Hybrid approach optimized for LLM-autonomous development:

**1. Programmatic drawing** for game entities (players, patients, stations):
```csharp
public override void _Draw()
{
    DrawCircle(Vector2.Zero, 16, _stateColor); // body
    DrawCircle(Vector2.Zero, 16, Colors.Black, false, 2f); // outline
}
```

**2. SVG icons** for semantic symbols (health, timer, station types):
```
assets/game-icons/lorc/health-normal.svg
assets/game-icons/delapouite/medical-pack.svg
assets/game-icons/lorc/hourglass.svg
```

**Finding icons:**
```bash
find assets/game-icons -name "*.svg" | xargs basename -a | grep -i "health\|medical"
```

**Why this approach:**
- `_Draw()` = 100% code, dynamic state colors, easy iteration
- SVG icons = semantic meaning, Godot rasterizes at import
- Both are text-based, fully LLM-controllable

## Godot Source Reference

`.research/godot/` contains Godot 4.5.1 source. Use it to understand how things work:

```bash
# Example: find how Area2D handles input
grep -r "input_pickable" .research/godot/scene/2d/
```

Key source paths:
- `scene/2d/` - 2D nodes (Area2D, CollisionShape2D, etc.)
- `scene/gui/` - Control nodes (ColorRect, Button, etc.)
- `modules/mono/` - C# bindings

## C# Script Pattern

```csharp
using Godot;

public partial class MyNode : Node2D
{
    public override void _Ready() { }
    public override void _Process(double delta) { }
}
```

Attach to nodes via scene `.tscn` files or editor.

## Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds


<!-- BEGIN BEADS INTEGRATION -->
## Issue Tracking with bd (beads)

**IMPORTANT**: This project uses **bd (beads)** for ALL issue tracking. Do NOT use markdown TODOs, task lists, or other tracking methods.

### Why bd?

- Dependency-aware: Track blockers and relationships between issues
- Git-friendly: Auto-syncs to JSONL for version control
- Agent-optimized: JSON output, ready work detection, discovered-from links
- Prevents duplicate tracking systems and confusion

### Quick Start

**Check for ready work:**

```bash
bd ready --json
```

**Create new issues:**

```bash
bd create "Issue title" --description="Detailed context" -t bug|feature|task -p 0-4 --json
bd create "Issue title" --description="What this issue is about" -p 1 --deps discovered-from:bd-123 --json
```

**Claim and update:**

```bash
bd update bd-42 --status in_progress --json
bd update bd-42 --priority 1 --json
```

**Complete work:**

```bash
bd close bd-42 --reason "Completed" --json
```

### Issue Types

- `bug` - Something broken
- `feature` - New functionality
- `task` - Work item (tests, docs, refactoring)
- `epic` - Large feature with subtasks
- `chore` - Maintenance (dependencies, tooling)

### Priorities

- `0` - Critical (security, data loss, broken builds)
- `1` - High (major features, important bugs)
- `2` - Medium (default, nice-to-have)
- `3` - Low (polish, optimization)
- `4` - Backlog (future ideas)

### Workflow for AI Agents

1. **Check ready work**: `bd ready` shows unblocked issues
2. **Claim your task**: `bd update <id> --status in_progress`
3. **Create feature branch**: `git checkout -b <ticket-id>-short-description`
   - Example: `git checkout -b hospital-game-mtu-player-system`
4. **Work on it**: Implement, test, document
5. **Discover new work?** Create linked issue:
   - `bd create "Found bug" --description="Details about what was found" -p 1 --deps discovered-from:<parent-id>`
6. **Provide test plan**: Give user a concise test plan to verify deliverables
7. **Wait for approval**: User MUST confirm deliverables work before closing
8. **Complete**: Only after user approval: `bd close <id> --reason "Done"`
9. **Merge branch**: After approval, merge to main and push

### Ticket Closure Protocol

**NEVER close tickets without user verification.**

Before requesting closure:
1. Ensure build passes (`dotnet build`)
2. Provide a clear **Test Plan** with steps user can follow
3. Wait for user to run the test plan and approve
4. Only then close the ticket

**Test Plan Format:**
```
## Test Plan for [ticket-id]

**What was built:** Brief summary

**How to test:**
1. Step one
2. Step two
3. Expected result

**Success criteria:** What "working" looks like
```

### Auto-Sync

bd automatically syncs with git:

- Exports to `.beads/issues.jsonl` after changes (5s debounce)
- Imports from JSONL when newer (e.g., after `git pull`)
- No manual export/import needed!

### Important Rules

- ✅ Use bd for ALL task tracking
- ✅ Always use `--json` flag for programmatic use
- ✅ Link discovered work with `discovered-from` dependencies
- ✅ Check `bd ready` before asking "what should I work on?"
- ❌ Do NOT create markdown TODO lists
- ❌ Do NOT use external issue trackers
- ❌ Do NOT duplicate tracking systems

For more details, see README.md and docs/QUICKSTART.md.

<!-- END BEADS INTEGRATION -->
