using System.Numerics;
using AetherPhysics.Collision.Shapes;
using AetherPhysics.Common;
using AetherPhysics.Dynamics;
using tp5.ParticleHandlers;

namespace tp5;

public class SimulationConfig
{
    public float DeltaTime { get; set; }
    public float? SavingDeltaTime { get; set; }

    public uint? MaxSteps { get; set; } = null;
    public double? MaxSimulationTime { get; set; } = null;

    public Rectangle Bounds { get; set; }

    private World physicsWorld = new(gravity: Vector2.Zero);
    private List<ParticleHandler> particleHandlers = new();

    public string? OutputFile { get; set; } = null;

    public void AddLineWall((float, float) start, (float, float) end)
    {
        physicsWorld.CreateBody().CreateFixture(new EdgeShape(new Vector2(start.Item1, start.Item2), new Vector2(end.Item1, end.Item2)));
    }

    public void AddRectangleWall(Rectangle rectangle)
    {
        Vertices vertices = new(4);
        vertices.Add(rectangle.BottomLeft);
        vertices.Add(rectangle.BottomRight);
        vertices.Add(rectangle.TopRight);
        vertices.Add(rectangle.TopLeft);

        physicsWorld.CreateBody().CreateFixture(new ChainShape(vertices, true));
    }

    public void AddParticleHandler(ParticleHandler handler)
    {
        particleHandlers.Add(handler);
    }

    public Simulation Build()
    {
        World physicsWorld2 = physicsWorld;
        List<ParticleHandler> particleHandlers2 = particleHandlers;

        // Not reusable
        physicsWorld = new World();
        particleHandlers = new List<ParticleHandler>();

        return new Simulation(DeltaTime, MaxSteps, physicsWorld2, Bounds, particleHandlers2);
    }
}