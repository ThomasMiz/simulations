using System.Globalization;
using System.Numerics;
using Silk.NET.Maths;
using tp5.DebugView;
using tp5.Integration;
using tp5.Particles;

namespace tp5;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // RunAllDeltaTimes();


        var config = new SimulationConfig
        {
            DeltaTime = 0.001,
            MaxSimulationTime = 5,
            OutputFile = "output-simple-{type}.txt",
            SavingDeltaTime = 0.1f,
            IntegrationMethod = new BeemanIntegration(),
        };

        config.AddParticle(new OsciladorAmortiguadoParticle()
        {
            Mass = 70,
            Radius = 0.25,
            Position = new Vector2D<double>(1, 0),
            Velocity = new Vector2D<double>(-1 * 100.0 / (2 * 70), 0),
            K = 10000,
            Y = 100,
        });

        using Simulation beeman = config.Build();

        using SimulationWindow window = new(simulation: beeman, simulationBounds: (new Vector2(-1, -1), new Vector2(1, 1)));
        window.SimulationSpeed = 1;
        window.Run();

        Console.WriteLine("Goodbye!");
    }
}