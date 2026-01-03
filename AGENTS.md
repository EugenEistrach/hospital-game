# Hospital Game - Agent Guide

## Quick Reference

| Task | Command |
|------|---------|
| Run game | `godot --path .` |
| Build C# | `dotnet build` |
| Format code | `dotnet format` |

## Project Structure

```
scenes/     → .tscn scene files
scripts/    → .cs C# scripts
```

## Key Files

- `project.godot` - Godot config (main scene: `scenes/main.tscn`)
- `HospitalGame.csproj` - .NET 8 project
- `orchestrator.json` - Dev scripts

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

## Godot CLI

```bash
godot --path .                    # Run game
godot --editor .                  # Open editor
godot --headless --build-solutions --quit  # CI build
```
