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
        Instance = this;
        GameLogger.Log("NetworkManager", "_Ready");

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }

    public Error HostGame(int port = DefaultPort)
    {
        // Note: run-multiplayer.sh clears logs before starting both instances
        GameLogger.SetRole("host", 1);

        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateServer(port, MaxPlayers);

        if (error != Error.Ok)
        {
            GameLogger.Log("NetworkManager", $"HostGame FAILED | Error={error}");
            return error;
        }

        Multiplayer.MultiplayerPeer = _peer;
        GameLogger.Log("NetworkManager", $"HostGame OK | Port={port} | MyId={Multiplayer.GetUniqueId()}");
        EmitSignal(SignalName.ServerStarted);
        return Error.Ok;
    }

    public Error JoinGame(string address, int port = DefaultPort)
    {
        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateClient(address, port);

        if (error != Error.Ok)
        {
            GameLogger.Log("NetworkManager", $"JoinGame FAILED | Error={error}");
            return error;
        }

        Multiplayer.MultiplayerPeer = _peer;
        GameLogger.Log("NetworkManager", $"JoinGame connecting | Address={address}:{port}");
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
        GameLogger.Log("NetworkManager", $"PeerConnected | PeerId={id}");
        EmitSignal(SignalName.PlayerConnected, id);
    }

    private void OnPeerDisconnected(long id)
    {
        GameLogger.Log("NetworkManager", $"PeerDisconnected | PeerId={id}");
        EmitSignal(SignalName.PlayerDisconnected, id);
    }

    private void OnConnectedToServer()
    {
        var myId = Multiplayer.GetUniqueId();
        GameLogger.SetRole("client", myId);
        GameLogger.Log("NetworkManager", $"ConnectedToServer | MyId={myId}");
        EmitSignal(SignalName.ConnectionSucceeded);
    }

    private void OnConnectionFailed()
    {
        GameLogger.Log("NetworkManager", "ConnectionFailed");
        Multiplayer.MultiplayerPeer = null;
        _peer = null;
        EmitSignal(SignalName.ConnectionFailed);
    }

    private void OnServerDisconnected()
    {
        GameLogger.Log("NetworkManager", "ServerDisconnected");
        Multiplayer.MultiplayerPeer = null;
        _peer = null;
    }
}
