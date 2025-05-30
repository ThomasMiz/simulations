using AetherPhysics.Dynamics;
using tp5.ParticleHandlers;

namespace tp5;

public class Simulation : IDisposable
{
    public float DeltaTime { get; }

    public uint? MaxSteps { get; }
    public uint Steps { get; private set; } = 0;
    public double SecondsElapsed => Steps * (double)DeltaTime;

    private List<ParticleHandler> particleHandlers = new();

    public World PhysicsWorld { get; private set; }

    public Rectangle Bounds { get; }

    public bool HasStopped { get; private set; } = false;

    public Simulation(float deltaTime, uint? maxSteps, World physicsWorld, Rectangle bounds, List<ParticleHandler> particleHandlers)
    {
        DeltaTime = deltaTime;
        MaxSteps = maxSteps;
        PhysicsWorld = physicsWorld;
        Bounds = bounds;
        this.particleHandlers = particleHandlers;
    }

    public void Initialize()
    {
        foreach (ParticleHandler particleHandler in particleHandlers)
        {
            particleHandler.Initialize(this);
        }
    }

    public void Step()
    {
        if (HasStopped) return;

        foreach (ParticleHandler particleHandler in particleHandlers)
        {
            particleHandler.PreUpdate(DeltaTime, SecondsElapsed);
        }

        PhysicsWorld.Step(DeltaTime);

        Steps++;

        foreach (ParticleHandler particleHandler in particleHandlers)
        {
            particleHandler.PostUpdate(DeltaTime, SecondsElapsed);
        }

        if (MaxSteps.HasValue && Steps >= MaxSteps.Value)
        {
            Console.WriteLine($"Stopping simulation after {Steps} steps; limit reached.");
            HasStopped = true;
        }
    }

    public void Dispose()
    {
    }
}