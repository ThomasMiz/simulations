using System.Numerics;
using tp5.ParticleHandlers;

namespace tp5;

class Program
{
    static void Main(string[] args)
    {
        const float HallwayWidth = 3.6f;
        const float HallwayLength = 16;

        const float halfSizeX = HallwayLength / 2;
        const float halfSizeY = HallwayWidth / 2;

        SimulationConfig config = new()
        {
            Bounds = new Rectangle(bottomLeft: (-halfSizeX, -halfSizeY), topRight: (halfSizeX, halfSizeY)),
            DeltaTime = 0.001f,
        };
        
        //config.AddRectangleWall(new Rectangle(bottomLeft: (-4, -2), topRight: (4, 2)));
        config.AddLineWall(start: (-(halfSizeX + 10), -halfSizeY), end: ((halfSizeX + 10), -halfSizeY));
        config.AddLineWall(start: (-(halfSizeX + 10), halfSizeY), end: ((halfSizeX + 10), halfSizeY));
        
        //config.AddParticleHandler(new ConstantForceAvoidingParticleHandler(spawnRate: 2f, force: 6f, maxEvasiveForce: 1.5f, particleRadius: 0.25f, particleSensingRadius: 0.75f, isLeftToRight: true));
        //config.AddParticleHandler(new ConstantForceAvoidingParticleHandler(spawnRate: 2f, force: 6f, maxEvasiveForce: 1.5f, particleRadius: 0.25f, particleSensingRadius: 0.75f, isLeftToRight: false));

        config.AddParticleHandler(new ConstantForceParticleHandler(spawnRate: 6f, force: 6f, particleRadius: 0.25f, isLeftToRight: true));
        config.AddParticleHandler(new ConstantForceParticleHandler(spawnRate: 6f, force: 6f, particleRadius: 0.25f, isLeftToRight: false));
        
        using Simulation simulation = config.Build();
        simulation.Initialize();
        
        using SimulationWindow window = new(simulation);
        window.Run();
    }
}