using Godot;

public partial class ClickableSquare : Area2D
{
    private ColorRect _colorRect = null!;
    private readonly Color _originalColor = new(0.2f, 0.5f, 0.8f);
    private readonly Color _clickColor = new(0.9f, 0.3f, 0.3f);

    public override void _Ready()
    {
        GD.Print("ClickableSquare ready!");
        _colorRect = GetNode<ColorRect>("ColorRect");
        _colorRect.Color = _originalColor;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mb)
        {
            var localPos = ToLocal(mb.Position);
            var rect = new Rect2(-50, -50, 100, 100);

            if (rect.HasPoint(localPos))
            {
                GD.Print("Square clicked!");
                FlashColor();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private async void FlashColor()
    {
        _colorRect.Color = _clickColor;
        await ToSignal(GetTree().CreateTimer(0.15), SceneTreeTimer.SignalName.Timeout);
        _colorRect.Color = _originalColor;
    }
}
