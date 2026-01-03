using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 300f;
    [Export] public int PlayerIndex = 0;

    // Colors for player 1 (green) and player 2 (blue)
    private static readonly Color[] PlayerColors = {
        new(0.2f, 0.7f, 0.3f),
        new(0.3f, 0.4f, 0.8f)
    };

    private Color _color = new(0.5f, 0.5f, 0.5f); // default gray if not set
    private bool _loggedFirstProcess = false;

    // Spawn points - must match Game.cs
    private static readonly Vector2[] SpawnPoints = {
        new(200, 300),
        new(200, 500)
    };

    public override void _Ready()
    {
        var myId = Multiplayer.GetUniqueId();

        // Parse peer ID from node name (format: "Player_<peerId>")
        // This ensures authority is correct on both server AND replicated clients
        var nameParts = Name.ToString().Split('_');
        if (nameParts.Length >= 2 && long.TryParse(nameParts[1], out var peerId))
        {
            SetMultiplayerAuthority((int)peerId);
            PlayerIndex = peerId == 1 ? 0 : 1;
            GameLogger.Log("Player", $"Parsed name | PeerId={peerId} | Index={PlayerIndex}");
        }
        else
        {
            GameLogger.Log("Player", $"WARN: Could not parse name '{Name}'");
        }

        _color = PlayerColors[PlayerIndex % PlayerColors.Length];

        // Set spawn position (critical for replicated nodes that don't have position set by server)
        // Only set if we're near origin (not already positioned)
        if (Position.LengthSquared() < 100)
        {
            Position = SpawnPoints[PlayerIndex];
            GameLogger.Log("Player", $"Set spawn position | Pos={Position}");
        }

        var isOurs = IsMultiplayerAuthority();
        GameLogger.Log("Player", $"Ready | Name={Name} | Index={PlayerIndex} | MyId={myId} | Authority={GetMultiplayerAuthority()} | IsOurs={isOurs} | Pos={Position}");

        // Force redraw with correct color
        QueueRedraw();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Log first physics process to debug authority
        if (!_loggedFirstProcess)
        {
            _loggedFirstProcess = true;
            var myId = Multiplayer.GetUniqueId();
            var auth = GetMultiplayerAuthority();
            var isOurs = IsMultiplayerAuthority();
            GameLogger.Log("Player", $"FirstProcess | Name={Name} | MyId={myId} | Auth={auth} | IsOurs={isOurs}");
        }

        // Only process input if this is our player (authority check for multiplayer)
        if (!IsMultiplayerAuthority())
            return;

        var velocity = Vector2.Zero;

        if (Input.IsActionPressed("ui_left"))
            velocity.X -= 1;
        if (Input.IsActionPressed("ui_right"))
            velocity.X += 1;
        if (Input.IsActionPressed("ui_up"))
            velocity.Y -= 1;
        if (Input.IsActionPressed("ui_down"))
            velocity.Y += 1;

        velocity = velocity.Normalized() * Speed;
        Velocity = velocity;
        MoveAndSlide();

        QueueRedraw();
    }

    public override void _Draw()
    {
        // Body circle
        DrawCircle(Vector2.Zero, 20, _color);
        // Outline
        DrawArc(Vector2.Zero, 20, 0, Mathf.Tau, 32, Colors.Black, 2f);
        // Direction indicator (small circle at front)
        DrawCircle(new Vector2(12, 0), 5, Colors.White);
    }

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
        _color = PlayerColors[index % PlayerColors.Length];
        QueueRedraw();
    }
}
