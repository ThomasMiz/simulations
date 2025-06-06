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

        const double spawnRate = 4;
        const double particleRadius = 0.25;

        const double spawnAreaLength = 1;

        Bounds simulationBounds = new(bottomLeft: (0, 0), topRight: (HallwayLength, HallwayWidth));

        var config = new SimulationConfig
        {
            DeltaTime = 10e-4,
            //MaxSimulationTime = 25,
            OutputFile = null, //"output-simple-{type}.txt",
            SavingDeltaTime = 0.1f,
            IntegrationMethod = new BeemanIntegration(),
            SimulationBounds = simulationBounds,
            MaxParticles = 200,
        };

        /*ParticleCreator spawnLeftToRightParticle = position => new TargetHorizontalVelocityParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = 1.5,
            Acceleration = 10,
            TargetX = halfSizeX - particleRadius * 1.2,
        };

        ParticleCreator spawnRightToLeftParticle = position => new TargetHorizontalVelocityParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = -1.5,
            Acceleration = 10,
            TargetX = -(halfSizeX - particleRadius * 1.2),
        };*/

        /*ParticleCreator spawnLeftToRightParticle = position => new SocialForceParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = 1.5,
            Acceleration = 10,
            TargetX = HallwayLength - spawnAreaLength,
        };

        ParticleCreator spawnRightToLeftParticle = position => new SocialForceParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = -1.5,
            Acceleration = 10,
            TargetX = spawnAreaLength,
        };*/

        ParticleCreator spawnLeftToRightParticle = position => new RightDrivingTrainParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = 1.5,
            Acceleration = 10,
            TargetX = HallwayLength - spawnAreaLength,
        };

        ParticleCreator spawnRightToLeftParticle = position => new RightDrivingTrainParticle()
        {
            Position = position,
            Radius = particleRadius,
            TargetHorizontalVelocity = -1.5,
            Acceleration = 10,
            TargetX = spawnAreaLength,
        };

        config.AddParticleSpawner(new GenericRateParticleSpawner(
            spawnRate: spawnRate,
            spawnArea: new Bounds(simulationBounds.Left, simulationBounds.Bottom + particleRadius, simulationBounds.Left + spawnAreaLength, simulationBounds.Top - particleRadius),
            particleCreator: spawnLeftToRightParticle
        ));

        config.AddParticleSpawner(new GenericRateParticleSpawner(
            spawnRate: spawnRate,
            spawnArea: new Bounds(simulationBounds.Right - spawnAreaLength, simulationBounds.Bottom + particleRadius, simulationBounds.Right, simulationBounds.Top - particleRadius),
            particleCreator: spawnRightToLeftParticle
        ));

        using Simulation sim = config.Build();
        using SimulationWindow window = new(sim);
        window.SimulationSpeed = 1;
        window.Run();

        Console.WriteLine("Goodbye!");
    }
}