using Godot;

/// <summary>
/// Global signal bus for game-wide events.
/// All game systems emit/listen to events through this singleton.
/// </summary>
public partial class GameEvents : Node
{
    public static GameEvents Instance { get; private set; } = null!;

    // Day lifecycle events
    [Signal] public delegate void DayStartedEventHandler(int dayNumber);
    [Signal] public delegate void DayEndedEventHandler(int dayNumber);
    [Signal] public delegate void DayPhaseChangedEventHandler(int phase); // 0=Prep, 1=Active, 2=Complete

    // Patient events
    [Signal] public delegate void PatientArrivedEventHandler(int patientId);
    [Signal] public delegate void PatientTreatedEventHandler(int patientId);
    [Signal] public delegate void PatientLeftEventHandler(int patientId, bool wasHealed);

    // Station events
    [Signal] public delegate void StationCompletedEventHandler(int stationId, int patientId);
    [Signal] public delegate void StationOccupiedEventHandler(int stationId, int patientId);
    [Signal] public delegate void StationFreedEventHandler(int stationId);

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[GameEvents] Signal bus ready");
    }
}
