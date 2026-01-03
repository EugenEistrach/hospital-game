using Godot;

public partial class Lobby : Control
{
    private Button _hostButton = null!;
    private Button _joinButton = null!;
    private LineEdit _ipInput = null!;
    private Label _statusLabel = null!;
    private Button _disconnectButton = null!;
    private Button _startGameButton = null!;

    private int _connectedPlayers = 0;

    public override void _Ready()
    {
        GD.Print("[Lobby] _Ready called");

        _hostButton = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/HostButton");
        _joinButton = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/JoinButton");
        _ipInput = GetNode<LineEdit>("CenterContainer/VBoxContainer/IpInput");
        _statusLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatusLabel");
        _disconnectButton = GetNode<Button>("CenterContainer/VBoxContainer/DisconnectButton");
        _startGameButton = GetNode<Button>("CenterContainer/VBoxContainer/StartGameButton");

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

        GD.Print("[Lobby] _Ready complete");
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
        _connectedPlayers++;

        if (NetworkManager.Instance.IsServer)
        {
            SetStatus($"Player {id} joined! ({_connectedPlayers}/2 players)", Colors.Green);
            // Show start button when we have 2 players (host counts as 1)
            if (_connectedPlayers >= 1) // 1 connected peer + host = 2 players
            {
                _startGameButton.Visible = true;
            }
        }
        _disconnectButton.Visible = true;
    }

    private void OnPlayerDisconnected(long id)
    {
        GD.Print($"[Lobby] OnPlayerDisconnected: {id}");
        _connectedPlayers--;
        SetStatus($"Player {id} left ({_connectedPlayers}/2 players)", Colors.Orange);

        if (_connectedPlayers < 1)
        {
            _startGameButton.Visible = false;
        }
    }

    // Connected via .tscn [connection]
    private void OnStartGamePressed()
    {
        GD.Print("[Lobby] OnStartGamePressed - host starting game");
        Rpc(MethodName.LoadGameScene);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void LoadGameScene()
    {
        GD.Print("[Lobby] Loading game scene...");
        GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
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
    }

    private void SetStatus(string text, Color color)
    {
        GD.Print($"[Lobby] Status: {text}");
        _statusLabel.Text = text;
        _statusLabel.Modulate = color;
    }
}
