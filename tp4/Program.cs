using System.Globalization;

namespace tp4;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        RunSimpleSystems();
        RunComplexSystem();
        RunEightLoopGravitySystem();
        RunDaisyChainGravitySystem();

        List<Action> runs = [RunSimpleSystems, RunComplexSystem, RunEightLoopGravitySystem, RunDaisyChainGravitySystem];
        Parallel.ForEach(runs, a => a());

        Console.WriteLine("Goodbye!");
    }

    private static void RunSimpleSystems()
    {
        // All mass units are in kg, all time units are in seconds
        const double A = 1;
        const double Mass = 70;
        const double K = 10000;
        const double Gamma = 100;

        var config = new SimulationConfig
        {
            DeltaTime = 0.01,
            MaxSimulationTime = 5,
            OutputFile = "simple-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: K, y: Gamma)
        }.AddParticle(mass: Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

        List<Action> runs =
        [
            () =>
            {
                using Simulation verlet = config.BuildVerlet();
                verlet.RunToEnd();
            },
            () =>
            {
                using Simulation beeman = config.BuildBeeman();
                beeman.RunToEnd();
            },
            () =>
            {
                using Simulation gear5 = config.BuildGear5();
                gear5.RunToEnd();
            },
        ];

        Parallel.ForEach(runs, a => a());
    }

    private static void RunComplexSystem()
    {
        // All mass units are in kg, all time units are in seconds
        const double m = 0.00021;
        const double k = 102.3; // NOTE: unit in kg/s2 is "corrected" interpreted as g/s2
        const double gamma = 0.0003;
        const double A = 0.01;
        const double l0 = 0.001;
        const double w = Math.PI * 2;
        const int N = 1000;

        var config = new SimulationConfig
        {
            DeltaTime = 0.001,
            MaxSimulationTime = 5,
            SaveEverySteps = 10,
            OutputFile = "complex-N{count}-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.OsciladoresAcoplados(k: k, y: gamma)
        };

        config.AddRailedParticle(mass: m, rail: new ParticleRails.OscillatorRail(a: A, w: w));
        for (int i = 1; i < N; i++)
            config.AddParticle(mass: m, position: (l0 * i, 0), velocity: (0, 0));

        List<Action> runs =
        [
            () =>
            {
                using Simulation verlet = config.BuildVerlet();
                verlet.RunToEnd();
            },
            () =>
            {
                using Simulation beeman = config.BuildBeeman();
                beeman.RunToEnd();
            },
            () =>
            {
                using Simulation gear5 = config.BuildGear5();
                gear5.RunToEnd();
            },
        ];

        Parallel.ForEach(runs, a => a());
    }

    private static void RunEightLoopGravitySystem()
    {
        var config = new SimulationConfig
        {
            DeltaTime = 0.001,
            MaxSimulationTime = 5,
            SaveEverySteps = 10,
            OutputFile = "gravityeightloop-N{count}-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.Gravity(g: 1)
        };

        config.AddParticle(mass: 1, position: (0.97000436, -0.24308753), velocity: (0.4662036850, 0.4323657300));
        config.AddParticle(mass: 1, position: (-0.97000436, 0.24308753), velocity: (0.4662036850, 0.4323657300));
        config.AddParticle(mass: 1, position: (0, 0), velocity: (-0.93240737, -0.86473146));

        using (Simulation sim = config.BuildBeeman())
        {
            sim.RunToEnd();
        }
    }

    private static void RunDaisyChainGravitySystem()
    {
        var config = new SimulationConfig
        {
            DeltaTime = 0.001,
            MaxSimulationTime = 9,
            SaveEverySteps = 10,
            OutputFile = "gravitydaisychain-N{count}-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.Gravity(g: 1)
        };

        const int N = 5;
        const double R = 1;
        const double S = 1.2;

        for (int i = 0; i < N; i++)
        {
            config.AddParticle(
                mass: 1,
                position: (R * Math.Cos(2 * Math.PI * i / N), R * Math.Sin(2 * Math.PI * i / N)),
                velocity: (-S * Math.Sin(2 * Math.PI * i / N), S * Math.Cos(2 * Math.PI * i / N))
            );
        }

        using (Simulation sim = config.BuildVerlet())
        {
            sim.RunToEnd();
        }
    }
}