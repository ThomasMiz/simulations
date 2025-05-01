namespace tp4;

public abstract class Simulation : IDisposable
{
    public string IntegrationType { get; }

    public float DeltaTime { get; }

    public uint? MaxSteps { get; } = null;

    public uint Steps { get; private set; } = 0;
    public float SecondsElapsed => Steps * DeltaTime;

    public ForceFunction ForceFunction { get; }

    protected ParticleConsts[] Consts { get; }

    protected int StepSaveCount { get; }
    protected List<ParticleState[]> States { get; } // Index 0 is current, higher indexes are older.

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
        ParticleState[] initialState = config.GetInitialStateArray();

        StepSaveCount = stepSaveCount;
        States = new List<ParticleState[]>(stepSaveCount);
        States.Add(initialState);

        if (outputFile != null)
        {
            saver = new SimulationFileSaver(outputFile, IntegrationType, DeltaTime, Consts);
            saver.AppendState(0, 0, initialState);
        }
    }

    public void Step()
    {
        StepImpl();

        Steps++;

        saver?.AppendState(Steps, SecondsElapsed, CurrentState);
    }

    public void RunToEnd()
    {
        Console.WriteLine("Running simulation...");

        while (true)
        {
            Step();

            if (MaxSteps.HasValue && Steps >= MaxSteps)
            {
                Console.WriteLine("Stopping simulation after {0} steps and {1} seconds; limit reached", Steps, SecondsElapsed);
                break;
            }

            if (Steps % 1000 == 0)
                Console.WriteLine("Ran {0} steps", Steps);
        }
    }

    protected abstract void StepImpl();

    public void Dispose()
    {
        saver?.Dispose();
    }
}