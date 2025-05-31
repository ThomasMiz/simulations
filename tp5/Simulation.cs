using Silk.NET.Maths;
using tp5.Integration;
using tp5.Particles;
using tp5.Spawners;

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

    public List<ParticleSpawner> ParticleSpawners { get; }
    public LinkedList<Particle> Particles { get; }
    private Queue<Particle> particleRemovalQueue = new();
    private long particleIdCounter = 1;

    private readonly NeighborsFinder neighborsFinder;
    private bool neighborsFinderDirty = true;

    private SimulationFileSaver? saver;
    
    private List<Particle> tmpParticles = new();

    public Simulation(IntegrationMethod integrationMethod, double deltaTime, uint? maxSteps, Bounds bounds, List<Particle> initialParticles, List<ParticleSpawner> particleSpawners, SimulationFileSaver? saver)
    {
        IntegrationMethod = integrationMethod;
        DeltaTime = deltaTime;
        MaxSteps = maxSteps;
        Bounds = bounds;
        this.saver = saver;

        ParticleSpawners = new List<ParticleSpawner>();
        Particles = new LinkedList<Particle>();

        foreach (ParticleSpawner particleSpawner in particleSpawners)
        {
            particleSpawner.Simulation = this;
            ParticleSpawners.Add(particleSpawner);
        }

        foreach (Particle particle in initialParticles)
        {
            particle.Id = particleIdCounter++;
            particle.Node = Particles.AddLast(particle);
            particle.Simulation = this;
        }

        neighborsFinder = new NeighborsFinder(Bounds.TopRight, new Vector2D<int>(1, 1), Particles);
    }

    private void Initialize()
    {
        neighborsFinder.Recalculate();

        Parallel.ForEach(Particles, particle => particle.OnInitialized());
        Parallel.ForEach(Particles, particle => IntegrationMethod.InitializeParticle(particle, DeltaTime));

        foreach (ParticleSpawner particleSpawner in ParticleSpawners)
        {
            particleSpawner.OnInitialized();
        }

        saver?.WriteStart(this);

        neighborsFinderDirty = true;
    }

    public void AddParticle(Particle particle)
    {
        particle.Id = particleIdCounter++;
        particle.Node = Particles.AddLast(particle);
        particle.Simulation = this;
        particle.OnInitialized();
        IntegrationMethod.InitializeParticle(particle, DeltaTime);

        if (!neighborsFinderDirty)
            neighborsFinder.ManualAddParticle(particle);
        // neighborsFinderDirty = true;
    }

    private void PerformRemoveParticle(Particle particle)
    {
        if (particle.Simulation == null) return;

        Particles.Remove(particle.Node);
        particle.Simulation = null;
        particle.Node = null;
        particle.OnRemoved();

        if (!neighborsFinderDirty)
            neighborsFinder.ManualRemoveParticle(particle);
        // neighborsFinderDirty = true;
    }

    public void RemoveParticle(Particle particle)
    {
        if (particle.Simulation != this) throw new Exception("Tried to remove a particle that is not from this sim");

        particleRemovalQueue.Enqueue(particle);
    }

    private void EnsureNeighborsUsable()
    {
        if (neighborsFinderDirty)
        {
            neighborsFinder.Recalculate();
            neighborsFinderDirty = false;
        }
    }
    
    public void FindParticlesWithinRadius(in Vector2D<double> position, double particleRadius, double distance, ICollection<Particle> result)
    {
        EnsureNeighborsUsable();
        neighborsFinder.FindWithinRadius(position, particleRadius, distance, result);
    }

    public void FindParticlesWithinRadius(Particle particle, double distance, ICollection<Particle> result)
    {
        FindParticlesWithinRadius(particle.Position, particle.Radius, distance, result);
        result.Remove(particle);
    }

    private void PostprocessNoCollisions()
    {
        tmpParticles.Clear();

        foreach (Particle particle in Particles)
        {
            FindParticlesWithinRadius(particle, 0, tmpParticles);
            foreach (Particle other in tmpParticles)
            {
                Vector2D<double> d = other.Position - particle.Position;
                double distance = d.Length;
                double overlapDistance = (particle.Radius + other.Radius - distance) / 2;
                double moveDistance = overlapDistance;
                Vector2D<double> dnorm = d / distance;
                other.Position += dnorm * moveDistance;
                particle.Position -= dnorm * moveDistance;
            }
            
            tmpParticles.Clear();
        }
    }

    public void Step()
    {
        if (HasStopped) return;

        if (Steps == 0)
        {
            Initialize();
        }

        EnsureNeighborsUsable();
        foreach (ParticleSpawner particleSpawner in ParticleSpawners)
        {
            particleSpawner.PreUpdate();
        }

        EnsureNeighborsUsable();
        
        Steps++;
        IntegrationMethod.Step(Particles, DeltaTime);

        neighborsFinderDirty = true;
        foreach (Particle particle in Particles)
        {
            particle.Position = particle.NextPosition;
            particle.Velocity = particle.NextVelocity;
            particle.PostUpdate();
        }

        neighborsFinderDirty = true;
        EnsureNeighborsUsable();
        PostprocessNoCollisions();
        neighborsFinderDirty = true;

        while (particleRemovalQueue.TryDequeue(out Particle particle))
        {
            PerformRemoveParticle(particle);
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