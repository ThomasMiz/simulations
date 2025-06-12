using System.Globalization;
using tp5.DebugView;
using tp5.Integration;
using tp5.Particles;
using tp5.Spawners;

namespace tp5;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        const double HallwayWidth = 3.6f;
        const double HallwayLength = 16;

        const double spawnRate = 8;
        const double particleRadius = 0.25;
        const uint maxParticles = 100;
        const double targetVelocity = 1.5;
        const double B = 0.08;

        const double spawnAreaLength = 1;

        Bounds simulationBounds = new(bottomLeft: (0, 0), topRight: (HallwayLength, HallwayWidth));

        var config = new SimulationConfig
        {
            DeltaTime = 10e-4,
            //MaxSimulationTime = 25,
            OutputFile = $"output-simple-Q{spawnRate}-B{B}-{{type}}-run.txt",
            SavingDeltaTime = 0.1f,
            IntegrationMethod = new BeemanIntegration(),
            SimulationBounds = simulationBounds,
            // MaxParticles = 200,
        };

        /*ParticleCreator spawnLeftToRightParticle = position => new TargetHorizontalVelocityParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = targetVelocity,
            TargetX = HallwayLength - spawnAreaLength,
            Mass = 80,
            Tao = 0.5,
        };

        ParticleCreator spawnRightToLeftParticle = position => new TargetHorizontalVelocityParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = -targetVelocity,
            TargetX = spawnAreaLength,
            Mass = 80,
            Tao = 0.5,
        };*/

        ParticleCreator spawnLeftToRightParticle = position => new SocialForceParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = targetVelocity,
            TargetX = HallwayLength - spawnAreaLength,
            Mass = 80,
            Tao = 0.5f,
            A = 2000,
            B = B,
            Kn = 1.2e5,
        };

        ParticleCreator spawnRightToLeftParticle = position => new SocialForceParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = -targetVelocity,
            TargetX = spawnAreaLength,
            Mass = 80,
            Tao = 0.5f,
            A = 2000,
            B = B,
            Kn = 1.2e5,
        };

        /*ParticleCreator spawnLeftToRightParticle = position => new TrainPromotingSfmParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = targetVelocity,
            TargetX = HallwayLength - spawnAreaLength,
            Mass = 80,
            Tao = 0.5f,
            A = 100,
            B = 0.3,
            Kn = 1.2e5,
        };

        ParticleCreator spawnRightToLeftParticle = position => new TrainPromotingSfmParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = -targetVelocity,
            TargetX = spawnAreaLength,
            Mass = 80,
            Tao = 0.5f,
            A = 100,
            B = 0.3,
            Kn = 1.2e5,
        };*/

        config.AddParticleSpawner(new GenericRateParticleSpawner(
            spawnRate: spawnRate,
            spawnArea: new Bounds(simulationBounds.Left, simulationBounds.Bottom + particleRadius, simulationBounds.Left + spawnAreaLength, simulationBounds.Top - particleRadius),
            particleCreator: spawnLeftToRightParticle,
            maxParticles: maxParticles
        ));

        config.AddParticleSpawner(new GenericRateParticleSpawner(
            spawnRate: spawnRate,
            spawnArea: new Bounds(simulationBounds.Right - spawnAreaLength, simulationBounds.Bottom + particleRadius, simulationBounds.Right, simulationBounds.Top - particleRadius),
            particleCreator: spawnRightToLeftParticle,
            maxParticles: maxParticles
        ));

        using Simulation sim = config.Build();
        using SimulationWindow window = new(sim);
        window.SimulationSpeed = 2;
        window.Run();

        Console.WriteLine("Goodbye!");
    }
}