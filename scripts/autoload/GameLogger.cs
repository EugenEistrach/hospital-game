using Godot;
using System;
using System.IO;

/// <summary>
/// File-based logging for multiplayer debugging.
/// Writes to logs/host.log or logs/client_<id>.log
/// </summary>
public partial class GameLogger : Node
{
    public static GameLogger Instance { get; private set; } = null!;

    private StreamWriter? _writer;
    private string _role = "unknown";
    private string _logPath = "";
    private readonly string _logsDir;

    public GameLogger()
    {
        _logsDir = ProjectSettings.GlobalizePath("res://logs");
    }

    public override void _Ready()
    {
        Instance = this;

        if (!Directory.Exists(_logsDir))
            Directory.CreateDirectory(_logsDir);

        // Start with temp file until role is known
        var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
        _logPath = Path.Combine(_logsDir, $"pending_{pid}.log");

        _writer = new StreamWriter(_logPath, false) { AutoFlush = true };
        Log("GameLogger", $"Session started | PID={pid}");
    }

    public override void _ExitTree()
    {
        _writer?.Close();
    }

    public static void SetRole(string role, long peerId = 0)
    {
        Ensure.NotNull(Instance, "GameLogger.Instance");

        Instance._role = role;
        var oldPath = Instance._logPath;

        var newName = role == "host" ? "host.log" : $"client_{peerId}.log";
        var newPath = Path.Combine(Instance._logsDir, newName);

        Instance._writer?.Close();

        if (File.Exists(newPath))
            File.Delete(newPath);

        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);

        Instance._logPath = newPath;
        Instance._writer = new StreamWriter(newPath, true) { AutoFlush = true };

        Log("GameLogger", $"Role: {role} | PeerId={peerId}");
    }

    public static void Log(string component, string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        var role = Instance?._role ?? "?";
        var line = $"[{ts}] [{role}] [{component}] {message}";

        // Write to file if available, always print to console
        Instance?._writer?.WriteLine(line);
        GD.Print(line);
    }
}
