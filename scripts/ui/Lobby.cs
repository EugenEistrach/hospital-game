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
        Ensure.NotNull(NetworkManager.Instance, "NetworkManager.Instance");

        _hostButton = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/HostButton");
        _joinButton = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/JoinButton");
        _ipInput = GetNode<LineEdit>("CenterContainer/VBoxContainer/IpInput");
        _statusLabel = GetNode<Label>("CenterContainer/VBoxContainer/StatusLabel");
        _disconnectButton = GetNode<Button>("CenterContainer/VBoxContainer/DisconnectButton");
        _startGameButton = GetNode<Button>("CenterContainer/VBoxContainer/StartGameButton");

        NetworkManager.Instance.PlayerConnected += OnPlayerConnected;
        NetworkManager.Instance.PlayerDisconnected += OnPlayerDisconnected;
        NetworkManager.Instance.ConnectionSucceeded += OnConnectionSucceeded;
        NetworkManager.Instance.ConnectionFailed += OnConnectionFailed;
        NetworkManager.Instance.ServerStarted += OnServerStarted;

        GD.Print("[Lobby] Ready");
    }

    private void OnHostPressed()
    {
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

    private void OnJoinPressed()
    {
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

    private void OnDisconnectPressed()
    {
        NetworkManager.Instance.Disconnect();
        SetStatus("Disconnected", Colors.White);
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
        _disconnectButton.Visible = false;
        _startGameButton.Visible = false;
        _connectedPlayers = 0;
    }

    private void OnPlayerConnected(long id)
    {
        GD.Print($"[Lobby] Player connected: {id}");
        _connectedPlayers++;

        if (NetworkManager.Instance.IsServer)
        {
            SetStatus($"Player {id} joined! ({_connectedPlayers + 1}/2 players)", Colors.Green);
            _startGameButton.Visible = true;
        }
        _disconnectButton.Visible = true;
    }

    private void OnPlayerDisconnected(long id)
    {
        GD.Print($"[Lobby] Player disconnected: {id}");
        _connectedPlayers--;
        SetStatus($"Player {id} left ({_connectedPlayers + 1}/2 players)", Colors.Orange);

        if (_connectedPlayers < 1)
        {
            _startGameButton.Visible = false;
        }
    }

    private void OnStartGamePressed()
    {
        GD.Print("[Lobby] Starting game...");
        Rpc(MethodName.LoadGameScene);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void LoadGameScene()
    {
        GD.Print("[Lobby] Loading game scene");
        GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
    }

    private void OnConnectionSucceeded()
    {
        GD.Print("[Lobby] Connected to server");
        SetStatus("Connected! Waiting for host to start...", Colors.Green);
        _disconnectButton.Visible = true;
    }

    private void OnConnectionFailed()
    {
        GD.Print("[Lobby] Connection failed");
        SetStatus("Connection failed!", Colors.Red);
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
    }

    private void OnServerStarted()
    {
        GD.Print("[Lobby] Server started");
        SetStatus("Server started, waiting for player...", Colors.Yellow);
    }

    private void SetStatus(string text, Color color)
    {
        _statusLabel.Text = text;
        _statusLabel.Modulate = color;
    }
}
