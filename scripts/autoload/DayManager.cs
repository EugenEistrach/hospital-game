using Godot;

/// <summary>
/// Manages the day/night cycle and game phases.
/// Server authoritative - clients observe state via sync.
/// </summary>
public partial class DayManager : Node
{
    public enum Phase { Preparation, DayActive, DayComplete }

    public static DayManager Instance { get; private set; } = null!;

    [Export] public float PreparationDuration { get; set; } = 5.0f;
    [Export] public float DayDuration { get; set; } = 60.0f;

    public int CurrentDay { get; private set; } = 0;
    public Phase CurrentPhase { get; private set; } = Phase.Preparation;
    public float PhaseTimeRemaining { get; private set; } = 0f;

    private bool _dayInProgress = false;

    public override void _Ready()
    {
        // Verify autoload dependencies (fail fast if order is wrong in project.godot)
        Ensure.NotNull(GameEvents.Instance, "GameEvents.Instance");
        Ensure.NotNull(NetworkManager.Instance, "NetworkManager.Instance");

        Instance = this;
        GD.Print("[DayManager] Ready");
    }

    public override void _Process(double delta)
    {
        if (!_dayInProgress) return;
        if (!NetworkManager.Instance.IsServer) return; // Only server ticks the clock

        PhaseTimeRemaining -= (float)delta;

        if (PhaseTimeRemaining <= 0)
        {
            AdvancePhase();
        }
    }

    /// <summary>
    /// Start a new day. Called by server when game begins or new day starts.
    /// </summary>
    public void StartDay()
    {
        if (!NetworkManager.Instance.IsServer)
        {
            GD.PrintErr("[DayManager] Only server can start day");
            return;
        }

        CurrentDay++;
        _dayInProgress = true;
        SetPhase(Phase.Preparation);

        GD.Print($"[DayManager] Day {CurrentDay} started - Preparation phase");
        RpcSyncDayState();
    }

    private void AdvancePhase()
    {
        switch (CurrentPhase)
        {
            case Phase.Preparation:
                SetPhase(Phase.DayActive);
                GD.Print($"[DayManager] Day {CurrentDay} - Active phase begins!");
                break;

            case Phase.DayActive:
                SetPhase(Phase.DayComplete);
                GD.Print($"[DayManager] Day {CurrentDay} - Complete!");
                break;

            case Phase.DayComplete:
                _dayInProgress = false;
                GameEvents.Instance?.EmitSignal(GameEvents.SignalName.DayEnded, CurrentDay);
                GD.Print($"[DayManager] Day {CurrentDay} ended");
                break;
        }

        RpcSyncDayState();
    }

    private void SetPhase(Phase newPhase)
    {
        CurrentPhase = newPhase;
        PhaseTimeRemaining = newPhase switch
        {
            Phase.Preparation => PreparationDuration,
            Phase.DayActive => DayDuration,
            Phase.DayComplete => 3.0f, // Brief pause before day end
            _ => 0f
        };

        GameEvents.Instance?.EmitSignal(GameEvents.SignalName.DayPhaseChanged, (int)newPhase);

        if (newPhase == Phase.Preparation)
        {
            GameEvents.Instance?.EmitSignal(GameEvents.SignalName.DayStarted, CurrentDay);
        }
    }

    private void RpcSyncDayState()
    {
        if (!NetworkManager.Instance.IsServer) return;
        Rpc(MethodName.ClientReceiveDayState, CurrentDay, (int)CurrentPhase, PhaseTimeRemaining);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    private void ClientReceiveDayState(int day, int phase, float timeRemaining)
    {
        var oldPhase = CurrentPhase;
        var oldDay = CurrentDay;

        CurrentDay = day;
        CurrentPhase = (Phase)phase;
        PhaseTimeRemaining = timeRemaining;
        _dayInProgress = CurrentPhase != Phase.DayComplete || timeRemaining > 0;

        // Emit signals so client-side listeners react
        if (oldDay != day && CurrentPhase == Phase.Preparation)
        {
            GameEvents.Instance?.EmitSignal(GameEvents.SignalName.DayStarted, day);
        }
        if (oldPhase != CurrentPhase)
        {
            GameEvents.Instance?.EmitSignal(GameEvents.SignalName.DayPhaseChanged, phase);
        }

        GD.Print($"[DayManager] Client synced: Day {day}, Phase {(Phase)phase}, Time {timeRemaining:F1}s");
    }

    /// <summary>
    /// Force end the current day early. Server only.
    /// </summary>
    public void EndDayEarly()
    {
        if (!NetworkManager.Instance.IsServer) return;
        SetPhase(Phase.DayComplete);
        PhaseTimeRemaining = 0.1f;
        RpcSyncDayState();
    }
}
