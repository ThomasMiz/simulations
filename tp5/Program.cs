using System.Globalization;
using System.Numerics;
using tp5.DebugView;
using tp5.Integration;

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
            OutputFile = null,//$"output-simple-{{type}}-{{steps}}steps-dt0.001.txt",
            ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: 10000, y: 100),
            IntegrationMethod = new BeemanIntegration(),
        }.AddParticle(mass: 70, position: (1, 0), velocity: (-1 * 100.0 / (2 * 70), 0));

        using Simulation beeman = config.Build();

        using SimulationWindow window = new(simulation: beeman, simulationBounds: (new Vector2(-1, -1), new Vector2(1, 1)));
        window.SimulationSpeed = 1;
        window.Run();

        Console.WriteLine("Goodbye!");
    }

    private static void RunAllDeltaTimes()
    {
        double[] deltaTimes = { 1e-6, 1e-5, 1e-4, 1e-3, 1e-2 };
        Parallel.ForEach(deltaTimes, RunSimpleSystems);
    }

    private static void RunSimpleSystems(double deltaTime)
    {
        // All mass units are in kg, all time units are in seconds
        const double A = 1;
        const double Mass = 70;
        const double K = 10000;
        const double Gamma = 100;

        List<Action> runs =
        [
            () =>
            {
                var config = new SimulationConfig
                {
                    DeltaTime = deltaTime,
                    MaxSimulationTime = 5,
                    OutputFile = $"output-simple-{{type}}-{{steps}}steps-dt{deltaTime:e0}.txt",
                    ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: K, y: Gamma),
                    IntegrationMethod = new BeemanIntegration(),
                }.AddParticle(mass: Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

                using Simulation beeman = config.Build();
                beeman.RunToEnd();
            },
            () =>
            {
                var config = new SimulationConfig
                {
                    DeltaTime = deltaTime,
                    MaxSimulationTime = 5,
                    OutputFile = $"output-simple-{{type}}-{{steps}}steps-dt{deltaTime:e0}.txt",
                    ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: K, y: Gamma),
                    IntegrationMethod = new Gear5Integration(),
                }.AddParticle(mass: Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

                using Simulation gear5 = config.Build();
                gear5.RunToEnd();
            },
        ];

        Parallel.ForEach(runs, a => a());
    }
}