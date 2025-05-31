using System.Globalization;

namespace tp5;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        RunAllDeltaTimes();

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
            /*() =>
            {
                using Simulation verlet = config.BuildVerlet();
                verlet.RunToEnd();
            },*/
            () =>
            {
                var config = new SimulationConfig
                {
                    DeltaTime = deltaTime,
                    MaxSimulationTime = 5,
                    OutputFile = $"output-simple-{{type}}-{{steps}}steps-dt{deltaTime:e0}.txt",
                    ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: K, y: Gamma)
                }.AddParticle(mass: Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

                using Simulation beeman = config.BuildBeeman();
                beeman.RunToEnd();
            },
            () =>
            {
                var config = new SimulationConfig
                {
                    DeltaTime = deltaTime,
                    MaxSimulationTime = 5,
                    OutputFile = $"output-simple-{{type}}-{{steps}}steps-dt{deltaTime:e0}.txt",
                    ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: K, y: Gamma)
                }.AddParticle(mass: Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

                using Simulation gear5 = config.BuildGear5();
                gear5.RunToEnd();
            },
        ];

        Parallel.ForEach(runs, a => a());
    }
}