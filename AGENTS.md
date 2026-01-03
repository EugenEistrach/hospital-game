# Hospital Game - Agent Guide

## Commands

```bash
# Run game
/Applications/Godot_mono.app/Contents/MacOS/Godot --path .

# Build C#
/usr/local/share/dotnet/dotnet build

# Format code
/usr/local/share/dotnet/dotnet format
```

## Structure

```
scenes/     → .tscn scene files
scripts/    → .cs C# scripts
```

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
