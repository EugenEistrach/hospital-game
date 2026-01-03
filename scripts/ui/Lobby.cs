using Godot;

public partial class Lobby : Control
{
    private Button _hostButton = null!;
    private Button _joinButton = null!;
    private LineEdit _ipInput = null!;
    private Label _statusLabel = null!;
    private Button _disconnectButton = null!;
    private Button _startDayButton = null!;
    private Label _dayStatusLabel = null!;

    public override void _Ready()
    {
        GD.Print("[Lobby] _Ready called");

        _hostButton = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/HostButton");
        _joinButton = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/JoinButton");
        _ipInput = GetNode<LineEdit>("CenterContainer/VBoxContainer/IpInput");
        _statusLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatusLabel");
        _disconnectButton = GetNode<Button>("CenterContainer/VBoxContainer/DisconnectButton");
        _startDayButton = GetNode<Button>("CenterContainer/VBoxContainer/StartDayButton");
        _dayStatusLabel = GetNode<Label>("CenterContainer/VBoxContainer/DayStatusLabel");

        GD.Print($"[Lobby] Nodes found - Host: {_hostButton != null}, Join: {_joinButton != null}");
        GD.Print($"[Lobby] HostButton.Disabled = {_hostButton?.Disabled}");

        // Connect to NetworkManager signals
        if (NetworkManager.Instance != null)
        {
            GD.Print("[Lobby] NetworkManager.Instance is available");
            NetworkManager.Instance.PlayerConnected += OnPlayerConnected;
            NetworkManager.Instance.PlayerDisconnected += OnPlayerDisconnected;
            NetworkManager.Instance.ConnectionSucceeded += OnConnectionSucceeded;
            NetworkManager.Instance.ConnectionFailed += OnConnectionFailed;
            NetworkManager.Instance.ServerStarted += OnServerStarted;
        }
        else
        {
            GD.PrintErr("[Lobby] NetworkManager.Instance is NULL!");
        }

        // Connect to GameEvents signals
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.DayStarted += OnDayStarted;
            GameEvents.Instance.DayEnded += OnDayEnded;
            GameEvents.Instance.DayPhaseChanged += OnDayPhaseChanged;
            GameEvents.Instance.PatientArrived += OnPatientArrived;
        }

        GD.Print("[Lobby] _Ready complete");
    }

    public override void _Process(double delta)
    {
        // Update day status display
        if (DayManager.Instance != null && DayManager.Instance.CurrentDay > 0)
        {
            var phase = DayManager.Instance.CurrentPhase;
            var time = DayManager.Instance.PhaseTimeRemaining;
            _dayStatusLabel.Text = $"Day {DayManager.Instance.CurrentDay} - {phase} ({time:F1}s)";
        }
    }

    // Connected via .tscn [connection]
    private void OnHostPressed()
    {
        GD.Print("[Lobby] OnHostPressed called");
        var error = NetworkManager.Instance.HostGame();
        if (error == Error.Ok)
        {
            SetStatus("Hosting... waiting for player", Colors.Yellow);
            _hostButton.Disabled = true;
            _joinButton.Disabled = true;
        }
        else
        {
            SetStatus($"Failed to host: {error}", Colors.Red);
        }
    }

    // Connected via .tscn [connection]
    private void OnJoinPressed()
    {
        GD.Print("[Lobby] OnJoinPressed called");
        var ip = _ipInput.Text.Trim();
        if (string.IsNullOrEmpty(ip))
            ip = "127.0.0.1";

        var error = NetworkManager.Instance.JoinGame(ip);
        if (error == Error.Ok)
        {
            SetStatus($"Connecting to {ip}...", Colors.Yellow);
            _hostButton.Disabled = true;
            _joinButton.Disabled = true;
        }
        else
        {
            SetStatus($"Failed to connect: {error}", Colors.Red);
        }
    }

    // Connected via .tscn [connection]
    private void OnDisconnectPressed()
    {
        GD.Print("[Lobby] OnDisconnectPressed called");
        NetworkManager.Instance.Disconnect();
        SetStatus("Disconnected", Colors.White);
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
        _disconnectButton.Visible = false;
    }

    private void OnPlayerConnected(long id)
    {
        GD.Print($"[Lobby] OnPlayerConnected: {id}");
        if (NetworkManager.Instance.IsServer)
        {
            SetStatus($"Player {id} joined! Game ready.", Colors.Green);
        }
        _disconnectButton.Visible = true;
    }

    private void OnPlayerDisconnected(long id)
    {
        GD.Print($"[Lobby] OnPlayerDisconnected: {id}");
        SetStatus($"Player {id} left", Colors.Orange);
    }

    private void OnConnectionSucceeded()
    {
        GD.Print("[Lobby] OnConnectionSucceeded");
        SetStatus("Connected to server!", Colors.Green);
        _disconnectButton.Visible = true;
    }

    private void OnConnectionFailed()
    {
        GD.Print("[Lobby] OnConnectionFailed");
        SetStatus("Connection failed!", Colors.Red);
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
    }

    private void OnServerStarted()
    {
        GD.Print("[Lobby] OnServerStarted");
        SetStatus("Server started, waiting for player...", Colors.Yellow);
        _startDayButton.Visible = true;
        _startDayButton.Text = "Start Day 1";
    }

    private void SetStatus(string text, Color color)
    {
        GD.Print($"[Lobby] Status: {text}");
        _statusLabel.Text = text;
        _statusLabel.Modulate = color;
    }

    // Connected via .tscn [connection]
    private void OnStartDayPressed()
    {
        GD.Print("[Lobby] OnStartDayPressed called");
        if (NetworkManager.Instance.IsServer)
        {
            DayManager.Instance.StartDay();
            _startDayButton.Visible = false;
        }
    }

    private void OnDayStarted(int dayNumber)
    {
        GD.Print($"[Lobby] Day {dayNumber} started!");
        _startDayButton.Visible = false;
    }

    private void OnDayEnded(int dayNumber)
    {
        GD.Print($"[Lobby] Day {dayNumber} ended!");
        _dayStatusLabel.Text = $"Day {dayNumber} complete!";

        // Show start day button for next day (server only)
        if (NetworkManager.Instance.IsServer)
        {
            _startDayButton.Visible = true;
            _startDayButton.Text = $"Start Day {dayNumber + 1}";
        }
    }

    private void OnDayPhaseChanged(int phase)
    {
        var phaseName = (DayManager.Phase)phase;
        GD.Print($"[Lobby] Phase changed to: {phaseName}");
    }

    private void OnPatientArrived(int patientId)
    {
        GD.Print($"[Lobby] Patient {patientId} arrived!");
    }
}
