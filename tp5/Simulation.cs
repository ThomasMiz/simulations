using tp5.Integration;
using tp5.Particles;

namespace tp5;

public class Simulation : IDisposable
{
    public IntegrationMethod IntegrationMethod { get; }

    public double DeltaTime { get; }

    public uint? MaxSteps { get; } = null;

    public uint Steps { get; private set; } = 0;
    public double SecondsElapsed => Steps * DeltaTime;

    public LinkedList<Particle> Particles { get; }

    private SimulationFileSaver? saver;

    public Simulation(IntegrationMethod integrationMethod, double deltaTime, uint? maxSteps, LinkedList<Particle> particles, SimulationFileSaver? saver)
    {
        IntegrationMethod = integrationMethod;
        DeltaTime = deltaTime;
        MaxSteps = maxSteps;
        Particles = particles;
        this.saver = saver;
        
        saver?.WriteStart(this);
    }

    private void Initialize()
    {
        foreach (Particle particle in Particles)
        {
            particle.Simulation = this;
        }
        
        Parallel.ForEach(Particles, particle => particle.OnInitialized());
        Parallel.ForEach(Particles, particle => IntegrationMethod.InitializeParticle(particle, DeltaTime));
    }

    public void Step()
    {
        if (Steps == 0)
        {
            Initialize();
        }

        Steps++;
        IntegrationMethod.Step(Particles, DeltaTime);
        
        foreach (Particle particle in Particles)
        {
            particle.Position = particle.NextPosition;
            particle.Velocity = particle.NextVelocity;
        }

        saver?.OnStep(this);
    }

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