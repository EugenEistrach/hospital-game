using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 300f;
    [Export] public int PlayerIndex = 0;

    private static readonly Color[] PlayerColors = {
        new(0.2f, 0.7f, 0.3f), // green
        new(0.3f, 0.4f, 0.8f)  // blue
    };

    private static readonly Vector2[] SpawnPoints = {
        new(200, 300),
        new(200, 500)
    };

    private Color _color;

    public override void _Ready()
    {
        // Parse peer ID from node name (format: "Player_<peerId>")
        var nameParts = Name.ToString().Split('_');
        Ensure.That(nameParts.Length >= 2, $"Player name must be 'Player_<peerId>', got '{Name}'");
        Ensure.That(long.TryParse(nameParts[1], out var peerId), $"Could not parse peer ID from '{Name}'");

        SetMultiplayerAuthority((int)peerId);
        PlayerIndex = peerId == 1 ? 0 : 1;
        _color = PlayerColors[PlayerIndex];

        // Set spawn position for replicated nodes (server sets before AddChild, but replicated nodes need this)
        if (Position.LengthSquared() < 100)
        {
            Position = SpawnPoints[PlayerIndex];
        }

        GameLogger.Log("Player", $"Ready | Name={Name} | Index={PlayerIndex} | Authority={peerId} | IsOurs={IsMultiplayerAuthority()}");
        QueueRedraw();
    }

    public override void _PhysicsProcess(double delta)
    {
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
        DrawCircle(Vector2.Zero, 20, _color);
        DrawArc(Vector2.Zero, 20, 0, Mathf.Tau, 32, Colors.Black, 2f);
        DrawCircle(new Vector2(12, 0), 5, Colors.White); // direction indicator
    }
}
