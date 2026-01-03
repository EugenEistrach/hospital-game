using Godot;
using System;

public partial class NetworkManager : Node
{
    public const int DefaultPort = 7000;
    public const int MaxPlayers = 2;

    [Signal] public delegate void PlayerConnectedEventHandler(long id);
    [Signal] public delegate void PlayerDisconnectedEventHandler(long id);
    [Signal] public delegate void ConnectionSucceededEventHandler();
    [Signal] public delegate void ConnectionFailedEventHandler();
    [Signal] public delegate void ServerStartedEventHandler();

    public static NetworkManager Instance { get; private set; } = null!;

    public bool IsServer => Multiplayer.IsServer();
    public bool IsNetworkConnected => Multiplayer.MultiplayerPeer?.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;

    private ENetMultiplayerPeer? _peer;

    public override void _Ready()
    {
        GD.Print("[NetworkManager] _Ready called - setting Instance");
        Instance = this;

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }

    public Error HostGame(int port = DefaultPort)
    {
        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateServer(port, MaxPlayers);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to create server: {error}");
            return error;
        }

        Multiplayer.MultiplayerPeer = _peer;
        GD.Print($"Server started on port {port}");
        EmitSignal(SignalName.ServerStarted);
        return Error.Ok;
    }

    public Error JoinGame(string address, int port = DefaultPort)
    {
        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateClient(address, port);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to create client: {error}");
            return error;
        }

        Multiplayer.MultiplayerPeer = _peer;
        GD.Print($"Connecting to {address}:{port}...");
        return Error.Ok;
    }

    public void Disconnect()
    {
        if (_peer != null)
        {
            _peer.Close();
            Multiplayer.MultiplayerPeer = null;
            _peer = null;
            GD.Print("Disconnected");
        }
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Player connected: {id}");
        EmitSignal(SignalName.PlayerConnected, id);
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Player disconnected: {id}");
        EmitSignal(SignalName.PlayerDisconnected, id);
    }

    private void OnConnectedToServer()
    {
        GD.Print("Connected to server!");
        EmitSignal(SignalName.ConnectionSucceeded);
    }

    private void OnConnectionFailed()
    {
        GD.Print("Connection failed!");
        Multiplayer.MultiplayerPeer = null;
        _peer = null;
        EmitSignal(SignalName.ConnectionFailed);
    }

    private void OnServerDisconnected()
    {
        GD.Print("Server disconnected!");
        Multiplayer.MultiplayerPeer = null;
        _peer = null;
    }
}
