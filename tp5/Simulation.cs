using tp5.Particles;

namespace tp5;

public abstract class Simulation : IDisposable
{
    public string IntegrationType { get; }

    public double DeltaTime { get; }

    public uint? MaxSteps { get; } = null;

    public uint Steps { get; private set; } = 0;
    public double SecondsElapsed => Steps * DeltaTime;

    public ForceFunction ForceFunction { get; }

    protected LinkedList<Particle> Particles { get; }

    private SimulationFileSaver? saver;

    protected Simulation(string integrationType, string? outputFile, SimulationConfig config)
    {
        IntegrationType = integrationType;
        DeltaTime = config.DeltaTime;
        MaxSteps = config.CalculateMaxSteps();
        ForceFunction = config.ForceFunction ?? throw new ArgumentNullException("config.ForceFunction");

        Particles = config.GetParticles();

        if (outputFile != null)
        {
            saver = new SimulationFileSaver(outputFile, config.SaveEverySteps, IntegrationType, DeltaTime, Particles);
            saver.AppendState(0, 0, Particles);
        }
    }

    private void Initialize()
    {
        InitializeImpl();
    }

    protected abstract void InitializeImpl();

    public void Step()
    {
        if (Steps == 0)
        {
            Initialize();
        }

        Steps++;
        StepImpl();
        
        foreach (Particle particle in Particles)
        {
            particle.Position = particle.NextPosition;
            particle.Velocity = particle.NextVelocity;
        }

        saver?.AppendState(Steps, SecondsElapsed, Particles);
    }

    protected abstract void StepImpl();

    public void RunToEnd()
    {
        Console.WriteLine("Running simulation...");

        while (true)
        {
            Step();

            bool badFloatDetected = Particles.Any(p =>
                !double.IsFinite(p.Position.X) || !double.IsFinite(p.Position.Y) || !double.IsFinite(p.Velocity.X) || !double.IsFinite(p.Velocity.Y)
            );

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