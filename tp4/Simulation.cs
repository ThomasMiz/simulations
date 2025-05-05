namespace tp4;

public abstract class Simulation : IDisposable
{
    public string IntegrationType { get; }

    public double DeltaTime { get; }

    public uint? MaxSteps { get; } = null;

    public uint Steps { get; private set; } = 0;
    public double SecondsElapsed => Steps * DeltaTime;

    public ForceFunction ForceFunction { get; }

    protected ParticleConsts[] Consts { get; }
    protected ParticleRail?[] Rails { get; }

    protected int StepSaveCount { get; }

    private readonly List<ParticleState[]> states; // Index 0 is current, higher indexes are older.
    protected IReadOnlyList<ParticleState[]> States => states;

    public ParticleState[] CurrentState => States[0];
    public ParticleState[] PreviousState => States[1];

    private SimulationFileSaver? saver;

    protected Simulation(string integrationType, int stepSaveCount, string? outputFile, SimulationConfig? config)
    {
        if (stepSaveCount <= 0) throw new ArgumentOutOfRangeException(nameof(stepSaveCount), stepSaveCount, "Must be > 0");

        IntegrationType = integrationType;
        DeltaTime = config.DeltaTime;
        MaxSteps = config.CalculateMaxSteps();
        ForceFunction = config.ForceFunction ?? throw new ArgumentNullException("config.ForceFunction");

        Consts = config.GetConstsArray();
        Rails = config.GetRailsArray();
        ParticleState[] initialState = config.GetInitialStateArray();

        StepSaveCount = stepSaveCount;
        states = new List<ParticleState[]>(stepSaveCount);
        states.Add(initialState);

        if (outputFile != null)
        {
            saver = new SimulationFileSaver(outputFile, config.SaveEverySteps, IntegrationType, DeltaTime, Consts);
            saver.AppendState(0, 0, initialState);
        }
    }

    private void Initialize()
    {
        InitializeImpl();

        while (states.Count < StepSaveCount)
            states.Add(new ParticleState[Consts.Length]);
    }

    protected abstract void InitializeImpl();

    protected void AddOlderState(ParticleState[] oldState)
    {
        states.Add(oldState);
    }

    public void Step()
    {
        if (Steps == 0)
        {
            Initialize();
        }

        Steps++;

        ParticleState[] nextState = states[^1];
        states.RemoveAt(states.Count - 1);
        StepImpl(nextState);
        states.Insert(0, nextState);

        saver?.AppendState(Steps, SecondsElapsed, CurrentState);
    }

    protected abstract void StepImpl(ParticleState[] nextState);

    public void RunToEnd()
    {
        Console.WriteLine("Running simulation...");

        while (true)
        {
            Step();

            bool badFloatDetected = false;
            for (int i = 0; !badFloatDetected && i < CurrentState.Length; i++)
            {
                badFloatDetected = !double.IsFinite(CurrentState[i].Position.X) || !double.IsFinite(CurrentState[i].Position.Y) || !double.IsFinite(CurrentState[i].Velocity.X) || !double.IsFinite(CurrentState[i].Velocity.Y);
            }

            if (badFloatDetected)
            {
                Console.WriteLine("WARNING! NaN or infinite value detected, stopping simulation after {0} steps", Steps);
                break;
            }

            if (MaxSteps.HasValue && Steps >= MaxSteps)
            {
                Console.WriteLine("Stopping simulation after {0} steps and {1} seconds; limit reached", Steps, SecondsElapsed);
                break;
            }

            if (Steps % 1000 == 0)
                Console.WriteLine("Ran {0} steps", Steps);
        }
    }

    public void Dispose()
    {
        saver?.Dispose();
    }
}