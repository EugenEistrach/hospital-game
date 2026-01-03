using Godot;
using System;
using System.IO;

public partial class GameLogger : Node
{
    public static GameLogger Instance { get; private set; } = null!;

    private StreamWriter? _writer;
    private string _role = "unknown";
    private string _logPath = "";
    private readonly string _logsDir;
    private readonly string _sessionId;

    public GameLogger()
    {
        _logsDir = ProjectSettings.GlobalizePath("res://logs");
        _sessionId = DateTime.Now.ToString("HHmmss");
    }

    public override void _Ready()
    {
        Instance = this;

        // Ensure logs directory exists
        if (!Directory.Exists(_logsDir))
            Directory.CreateDirectory(_logsDir);

        // Start with a temp file until we know our role
        var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
        _logPath = Path.Combine(_logsDir, $"pending_{pid}.log");

        _writer = new StreamWriter(_logPath, false) { AutoFlush = true };
        Log("GameLogger", $"Session started | PID={pid} | SessionId={_sessionId}");
    }

    public override void _ExitTree()
    {
        _writer?.Close();
    }

    public static void SetRole(string role, long peerId = 0)
    {
        if (Instance == null) return;

        Instance._role = role;
        var oldPath = Instance._logPath;

        // Rename to role-based filename
        var newName = role == "host" ? "host.log" : $"client_{peerId}.log";
        var newPath = Path.Combine(Instance._logsDir, newName);

        Instance._writer?.Close();

        // Delete existing file with same name (fresh logs)
        if (File.Exists(newPath))
            File.Delete(newPath);

        File.Move(oldPath, newPath);
        Instance._logPath = newPath;
        Instance._writer = new StreamWriter(newPath, true) { AutoFlush = true };

        Log("GameLogger", $"Role set: {role} | PeerId={peerId}");
    }

    public static void Log(string component, string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        var role = Instance?._role ?? "?";
        var line = $"[{ts}] [{role}] [{component}] {message}";

        Instance?._writer?.WriteLine(line);
        GD.Print(line);
    }

    public static void ClearOldLogs()
    {
        if (Instance == null) return;

        try
        {
            foreach (var file in Directory.GetFiles(Instance._logsDir, "*.log"))
            {
                // Don't delete our own pending file
                if (file != Instance._logPath)
                    File.Delete(file);
            }
            Log("GameLogger", "Cleared old log files");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to clear logs: {e.Message}");
        }
    }
}
