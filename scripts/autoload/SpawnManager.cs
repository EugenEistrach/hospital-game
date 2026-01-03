using Godot;

/// <summary>
/// Controls patient spawning during active day phase.
/// Server authoritative - spawns patients at intervals during DayActive.
/// </summary>
public partial class SpawnManager : Node
{
    public static SpawnManager Instance { get; private set; } = null!;

    [Export] public float SpawnInterval { get; set; } = 8.0f;
    [Export] public int MaxPatientsPerDay { get; set; } = 5;

    private float _spawnTimer = 0f;
    private int _patientsSpawnedThisDay = 0;
    private int _nextPatientId = 1;
    private bool _spawningActive = false;

    public override void _Ready()
    {
        // Verify autoload dependencies (fail fast if order is wrong in project.godot)
        Ensure.NotNull(GameEvents.Instance, "GameEvents.Instance");
        Ensure.NotNull(NetworkManager.Instance, "NetworkManager.Instance");
        Ensure.NotNull(DayManager.Instance, "DayManager.Instance");

        Instance = this;

        // Listen for day phase changes
        GameEvents.Instance.DayPhaseChanged += OnDayPhaseChanged;
        GameEvents.Instance.DayStarted += OnDayStarted;

        GD.Print("[SpawnManager] Ready");
    }

    public override void _Process(double delta)
    {
        if (!_spawningActive) return;
        if (!NetworkManager.Instance.IsServer) return;

        _spawnTimer -= (float)delta;

        if (_spawnTimer <= 0 && _patientsSpawnedThisDay < MaxPatientsPerDay)
        {
            SpawnPatient();
            _spawnTimer = SpawnInterval;
        }
    }

    private void OnDayStarted(int dayNumber)
    {
        _patientsSpawnedThisDay = 0;
        _spawnTimer = 2.0f; // Small delay before first spawn
        GD.Print($"[SpawnManager] Day {dayNumber} - Reset spawn counter");
    }

    private void OnDayPhaseChanged(int phase)
    {
        var dayPhase = (DayManager.Phase)phase;
        _spawningActive = dayPhase == DayManager.Phase.DayActive;

        if (_spawningActive)
        {
            GD.Print("[SpawnManager] Spawning activated");
        }
        else
        {
            GD.Print("[SpawnManager] Spawning deactivated");
        }
    }

    private void SpawnPatient()
    {
        var patientId = _nextPatientId++;
        _patientsSpawnedThisDay++;

        GD.Print($"[SpawnManager] Spawning patient {patientId} ({_patientsSpawnedThisDay}/{MaxPatientsPerDay})");

        // Emit event for other systems to react
        GameEvents.Instance.EmitSignal(GameEvents.SignalName.PatientArrived, patientId);

        // Notify clients
        Rpc(MethodName.ClientPatientSpawned, patientId);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    private void ClientPatientSpawned(int patientId)
    {
        GD.Print($"[SpawnManager] Client: Patient {patientId} spawned");
        GameEvents.Instance.EmitSignal(GameEvents.SignalName.PatientArrived, patientId);
    }
}
