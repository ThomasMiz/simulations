using System.Numerics;
using tp5.ParticleHandlers;

namespace tp5;

class Program
{
    static void Main(string[] args)
    {
        SimulationConfig config = new()
        {
            Bounds = new Rectangle(bottomLeft: (-8, -3), topRight: (8, 3)),
            DeltaTime = 0.01f,
        };
        
        //config.AddRectangleWall(new Rectangle(bottomLeft: (-4, -2), topRight: (4, 2)));
        config.AddLineWall(start: (-100, -3), end: (100, -3));
        config.AddLineWall(start: (-100, 3), end: (100, 3));
        
        config.AddParticleHandler(new ConstantForceAvoidingParticleHandler(spawnRate: 4f, force: 1.5f, maxEvasiveForce: 1.5f, particleRadius: 0.25f, particleSensingRadius: 0.75f, isLeftToRight: true));
        config.AddParticleHandler(new ConstantForceAvoidingParticleHandler(spawnRate: 4f, force: 1.5f, maxEvasiveForce: 1.5f, particleRadius: 0.25f, particleSensingRadius: 0.75f, isLeftToRight: false));

        using Simulation simulation = config.Build();
        simulation.Initialize();
        
        using SimulationWindow window = new(simulation);
        window.Run();
    }
}