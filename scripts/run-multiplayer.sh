#!/bin/bash
# Run two game instances in tmux split panes

SESSION="hospital-game"
GODOT="/Applications/Godot_mono.app/Contents/MacOS/Godot"
PROJECT_PATH="$(cd "$(dirname "$0")/.." && pwd)"

# Build first
echo "Building C#..."
/usr/local/share/dotnet/dotnet build "$PROJECT_PATH" || exit 1

# Clear old logs
echo "Clearing logs..."
rm -rf "$PROJECT_PATH/logs"
mkdir -p "$PROJECT_PATH/logs"

# Kill existing session if any
tmux kill-session -t "$SESSION" 2>/dev/null

# Create new tmux session with first instance (Host)
tmux new-session -d -s "$SESSION" -n "game" \
    "echo '=== PLAYER 1 (Host) ===' && $GODOT --path '$PROJECT_PATH' --position 100,100; read"

# Split and run second instance (Client)
tmux split-window -h -t "$SESSION" \
    "sleep 1 && echo '=== PLAYER 2 (Client) ===' && $GODOT --path '$PROJECT_PATH' --position 750,100; read"

# Attach to session
tmux attach -t "$SESSION"
