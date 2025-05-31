using Silk.NET.Maths;
using tp5.Integration;
using tp5.Particles;

namespace tp5;

public class Simulation : IDisposable
{
    public IntegrationMethod IntegrationMethod { get; }

    public double DeltaTime { get; }

    public Bounds Bounds { get; }

    public uint? MaxSteps { get; set; } = null;

    public uint Steps { get; private set; } = 0;
    public double SecondsElapsed => Steps * DeltaTime;

    public bool HasStopped { get; set; } = false;

    public LinkedList<Particle> Particles { get; }
    private long particleIdCounter = 1;

    private readonly NeighborsFinder neighborsFinder;
    private bool neighborsFinderDirty = true;

    private SimulationFileSaver? saver;

    public Simulation(IntegrationMethod integrationMethod, double deltaTime, uint? maxSteps, Bounds bounds, LinkedList<Particle> particles, SimulationFileSaver? saver)
    {
        IntegrationMethod = integrationMethod;
        DeltaTime = deltaTime;
        MaxSteps = maxSteps;
        Bounds = bounds;
        Particles = particles;
        this.saver = saver;

        saver?.WriteStart(this);
    }

    private void Initialize()
    {
        foreach (Particle particle in Particles)
        {
            particle.Id = particleIdCounter++;
            particle.Simulation = this;
        }

        Parallel.ForEach(Particles, particle => particle.OnInitialized());
        Parallel.ForEach(Particles, particle => IntegrationMethod.InitializeParticle(particle, DeltaTime));

        neighborsFinderDirty = true;
    }

    public void AddParticle(Particle particle)
    {
        particle.Id = particleIdCounter++;
        particle.Node = Particles.AddLast(particle);
        particle.Simulation = this;
        particle.OnInitialized();
        IntegrationMethod.InitializeParticle(particle, DeltaTime);

        neighborsFinderDirty = true;
    }

    public void RemoveParticle(Particle particle)
    {
        if (particle.Simulation != this) throw new Exception("Tried to remove a particle that is not from this sim");

        Particles.Remove(particle.Node);
        particle.Simulation = null;
        particle.Node = null;
        particle.OnRemoved();

        neighborsFinderDirty = true;
    }

    public void FindParticlesWithinRadius(in Vector2D<double> position, double particleRadius, double distance, ICollection<Particle> result)
    {
        if (neighborsFinderDirty)
        {
            neighborsFinder.Recalculate();
            neighborsFinderDirty = false;
        }

        neighborsFinder.FindWithinRadius(position, particleRadius, distance, result);
    }

    public void Step()
    {
        if (HasStopped) return;

        if (Steps == 0)
        {
            Initialize();
        }

        Steps++;
        IntegrationMethod.Step(Particles, DeltaTime);

        neighborsFinderDirty = true;

        foreach (Particle particle in Particles)
        {
            particle.Position = particle.NextPosition;
            particle.Velocity = particle.NextVelocity;
        }

        saver?.OnStep(this);

        bool badFloatDetected = Particles.Any(p =>
            !double.IsFinite(p.Position.X) || !double.IsFinite(p.Position.Y) || !double.IsFinite(p.Velocity.X) || !double.IsFinite(p.Velocity.Y)
        );

        if (badFloatDetected)
        {
            Console.WriteLine("WARNING! NaN or infinite value detected, stopping simulation after {0} steps", Steps);
            HasStopped = true;
        }
        else if (MaxSteps.HasValue && Steps >= MaxSteps)
        {
            Console.WriteLine("Stopping simulation after {0} steps and {1} seconds; limit reached", Steps, SecondsElapsed);
            HasStopped = true;
        }
        else if (Steps % 1000 == 0)
            Console.WriteLine("Ran {0} steps", Steps);
    }

    public void RunToEnd()
    {
        if (MaxSteps == null)
            throw new ArgumentException("Simulation has no end condition! Specify either MaxSteps or MaxSimulationTime.");

        Console.WriteLine("Running simulation...");

        while (!HasStopped)
        {
            Step();
        }
    }

    public void Dispose()
    {
        saver?.Dispose();
    }
}