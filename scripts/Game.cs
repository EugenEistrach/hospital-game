using Godot;
using System.Collections.Generic;

public partial class Game : Node2D
{
    [Export] public PackedScene PlayerScene = null!;

    private static readonly Vector2[] SpawnPoints = {
        new(200, 300),
        new(200, 500)
    };

    private Node _playersContainer = null!;
    private Label _dayStatusLabel = null!;
    private Button _startDayButton = null!;
    private readonly Dictionary<long, Player> _players = new();

    public override void _Ready()
    {
        Ensure.NotNull(PlayerScene, nameof(PlayerScene));
        Ensure.NotNull(GameEvents.Instance, "GameEvents.Instance");
        Ensure.NotNull(NetworkManager.Instance, "NetworkManager.Instance");
        Ensure.NotNull(DayManager.Instance, "DayManager.Instance");

        _playersContainer = GetNode<Node>("Players");
        _dayStatusLabel = GetNode<Label>("UI/DayStatusLabel");
        _startDayButton = GetNode<Button>("UI/StartDayButton");

        // Connect to GameEvents
        GameEvents.Instance.DayStarted += OnDayStarted;
        GameEvents.Instance.DayEnded += OnDayEnded;
        GameEvents.Instance.DayPhaseChanged += OnDayPhaseChanged;
        GameEvents.Instance.PatientArrived += OnPatientArrived;

        // Only server spawns players
        if (Multiplayer.IsServer())
        {
            SpawnAllPlayers();
            _startDayButton.Visible = true;
        }

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;

        GameLogger.Log("Game", $"Ready | IsServer={Multiplayer.IsServer()} | Peers=[{string.Join(",", Multiplayer.GetPeers())}]");
    }

    public override void _Process(double delta)
    {
        // Update day status display
        if (DayManager.Instance.CurrentDay > 0)
        {
            var phase = DayManager.Instance.CurrentPhase;
            var time = DayManager.Instance.PhaseTimeRemaining;
            _dayStatusLabel.Text = $"Day {DayManager.Instance.CurrentDay} | {phase} | {time:F0}s";
        }
    }

    private void SpawnAllPlayers()
    {
        SpawnPlayer(1); // Server
        foreach (var peerId in Multiplayer.GetPeers())
        {
            SpawnPlayer(peerId);
        }
    }

    private void SpawnPlayer(long peerId)
    {
        if (_players.ContainsKey(peerId))
            return;

        var player = PlayerScene.Instantiate<Player>();
        player.Name = $"Player_{peerId}";
        player.Position = SpawnPoints[peerId == 1 ? 0 : 1];

        _playersContainer.AddChild(player, true);
        _players[peerId] = player;

        GameLogger.Log("Game", $"SpawnPlayer | PeerId={peerId} | Pos={player.Position}");
    }

    private void OnPeerConnected(long id)
    {
        if (Multiplayer.IsServer())
            SpawnPlayer(id);
    }

    private void OnPeerDisconnected(long id)
    {
        if (_players.TryGetValue(id, out var player))
        {
            player.QueueFree();
            _players.Remove(id);
            GameLogger.Log("Game", $"Player removed | PeerId={id}");
        }
    }

    // UI callback
    private void OnStartDayPressed()
    {
        if (Multiplayer.IsServer())
        {
            DayManager.Instance.StartDay();
        }
    }

    private void OnDayStarted(int dayNumber)
    {
        _startDayButton.Visible = false;
        GameLogger.Log("Game", $"Day {dayNumber} started");
    }

    private void OnDayEnded(int dayNumber)
    {
        _dayStatusLabel.Text = $"Day {dayNumber} complete!";
        if (Multiplayer.IsServer())
        {
            _startDayButton.Visible = true;
            _startDayButton.Text = $"Start Day {dayNumber + 1}";
        }
        GameLogger.Log("Game", $"Day {dayNumber} ended");
    }

    private void OnDayPhaseChanged(int phase)
    {
        var phaseName = (DayManager.Phase)phase;
        GameLogger.Log("Game", $"Phase changed: {phaseName}");
    }

    private void OnPatientArrived(int patientId)
    {
        GameLogger.Log("Game", $"Patient {patientId} arrived");
        // TODO: Spawn patient entity
    }

    public override void _Draw()
    {
        // Hospital floor
        DrawRect(new Rect2(50, 50, 1180, 620), new Color(0.9f, 0.9f, 0.85f));
        DrawRect(new Rect2(50, 50, 1180, 620), new Color(0.3f, 0.3f, 0.3f), false, 4f);

        // Grid
        var gridColor = new Color(0.8f, 0.8f, 0.75f);
        for (int x = 150; x < 1230; x += 100)
            DrawLine(new Vector2(x, 50), new Vector2(x, 670), gridColor, 1f);
        for (int y = 150; y < 670; y += 100)
            DrawLine(new Vector2(50, y), new Vector2(1230, y), gridColor, 1f);

        // Spawn indicators
        DrawCircle(SpawnPoints[0], 30, new Color(0.2f, 0.6f, 0.2f, 0.3f));
        DrawCircle(SpawnPoints[1], 30, new Color(0.2f, 0.2f, 0.6f, 0.3f));
    }
}
