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
scenes/       → .tscn scene files
scripts/      → .cs C# scripts
.research/    → Godot 4.5.1 source (gitignored) - read to understand engine internals
```

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
