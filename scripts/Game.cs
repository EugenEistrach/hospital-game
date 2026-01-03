using Godot;
using System.Collections.Generic;

public partial class Game : Node2D
{
    [Export] public PackedScene PlayerScene = null!;

    // Spawn points for 2 players
    private static readonly Vector2[] SpawnPoints = {
        new(200, 300),
        new(200, 500)
    };

    private Node _playersContainer = null!;
    private readonly Dictionary<long, Player> _players = new();

    public override void _Ready()
    {
        _playersContainer = GetNode<Node>("Players");

        var myId = Multiplayer.GetUniqueId();
        var isServer = Multiplayer.IsServer();
        var peers = Multiplayer.GetPeers();
        GameLogger.Log("Game", $"_Ready | MyId={myId} | IsServer={isServer} | Peers=[{string.Join(",", peers)}]");

        // Only server spawns players
        if (isServer)
        {
            SpawnAllPlayers();
        }

        // Listen for new connections (for late joiners)
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    private void SpawnAllPlayers()
    {
        // Spawn for server (peer id 1)
        SpawnPlayer(1);

        // Spawn for all connected peers
        foreach (var peerId in Multiplayer.GetPeers())
        {
            SpawnPlayer(peerId);
        }
    }

    private void SpawnPlayer(long peerId)
    {
        if (_players.ContainsKey(peerId))
        {
            GameLogger.Log("Game", $"SpawnPlayer SKIP (exists) | PeerId={peerId}");
            return;
        }

        var player = PlayerScene.Instantiate<Player>();
        player.Name = $"Player_{peerId}";

        // Determine player index (0 for server, 1 for client)
        int playerIndex = peerId == 1 ? 0 : 1;
        player.Position = SpawnPoints[playerIndex];

        // Note: Authority will be set in Player._Ready() based on name
        // This ensures it works for both server-spawned and replicated nodes

        GameLogger.Log("Game", $"SpawnPlayer | PeerId={peerId} | Name={player.Name} | Pos={player.Position}");

        _playersContainer.AddChild(player, true);
        _players[peerId] = player;
    }

    private void OnPeerConnected(long id)
    {
        if (Multiplayer.IsServer())
        {
            SpawnPlayer(id);
        }
    }

    private void OnPeerDisconnected(long id)
    {
        if (_players.TryGetValue(id, out var player))
        {
            player.QueueFree();
            _players.Remove(id);
            GD.Print($"[Game] Removed player for peer {id}");
        }
    }

    public override void _Draw()
    {
        // Draw hospital floor
        DrawRect(new Rect2(50, 50, 1180, 620), new Color(0.9f, 0.9f, 0.85f)); // cream floor
        DrawRect(new Rect2(50, 50, 1180, 620), new Color(0.3f, 0.3f, 0.3f), false, 4f); // border

        // Draw grid lines for visual reference
        var gridColor = new Color(0.8f, 0.8f, 0.75f);
        for (int x = 150; x < 1230; x += 100)
            DrawLine(new Vector2(x, 50), new Vector2(x, 670), gridColor, 1f);
        for (int y = 150; y < 670; y += 100)
            DrawLine(new Vector2(50, y), new Vector2(1230, y), gridColor, 1f);

        // Draw spawn area indicators
        DrawCircle(SpawnPoints[0], 30, new Color(0.2f, 0.6f, 0.2f, 0.3f));
        DrawCircle(SpawnPoints[1], 30, new Color(0.2f, 0.2f, 0.6f, 0.3f));
    }

    public Vector2 GetSpawnPoint(int playerIndex)
    {
        return SpawnPoints[playerIndex % SpawnPoints.Length];
    }
}
