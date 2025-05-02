using System.Globalization;
using System.Numerics;
using TrippyGL.Utils;

namespace tp4;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        RunSimpleSystems();
        // RunComplexSystem();
        // RunEightLoopGravitySystem();
        RunDaisyChainGravitySystem();

        Console.WriteLine("Goodbye!");
    }

    private static void RunSimpleSystems()
    {
        // All mass units are in kg, all time units are in seconds
        const float A = 1f;
        const float Mass = 70f;
        const float K = 10000f;
        const float Gamma = 100;

        var config = new SimulationConfig()
        {
            DeltaTime = 0.01f,
            MaxSimulationTime = 5,
            OutputFile = "output-simple-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.OsciladorAmortiguado(k: K, y: Gamma),
        }.AddParticle(Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

        using (Simulation verlet = config.BuildVerlet())
        {
            verlet.RunToEnd();
        }

        using (Simulation beeman = config.BuildBeeman())
        {
            beeman.RunToEnd();
        }

        using (Simulation gear5 = config.BuildGear5())
        {
            gear5.RunToEnd();
        }
    }

    private static void RunComplexSystem()
    {
        // All mass units are in kg, all time units are in seconds
        const float m = 0.00021f;
        const float k = 0.1023f;
        const float gamma = 0.0003f;
        const float A = 0.01f;
        const float l0 = 0.001f;
        const float w = TrippyMath.TwoPI;
        const int N = 1000;

        var config = new SimulationConfig()
        {
            DeltaTime = 0.01f,
            MaxSimulationTime = 5,
            OutputFile = "output-N{count}-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.OsciladoresAcoplados(k: k, y: gamma),
        };

        config.AddRailedParticle(mass: m, rail: new ParticleRails.OscillatorRail(a: A, w: w));
        for (int i = 1; i < N; i++)
            config.AddParticle(mass: m, position: (l0 * i, 0), velocity: (0, 0));

        using (Simulation verlet = config.BuildVerlet())
        {
            verlet.RunToEnd();
        }

        using (Simulation beeman = config.BuildBeeman())
        {
            beeman.RunToEnd();
        }

        using (Simulation gear5 = config.BuildGear5())
        {
            gear5.RunToEnd();
        }
    }

    private static void RunEightLoopGravitySystem()
    {
        var config = new SimulationConfig()
        {
            DeltaTime = 0.001f,
            MaxSimulationTime = 5,
            SaveEverySteps = 10,
            OutputFile = "gravityeightloop-N{count}-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.Gravity(g: 1),
        };

        config.AddParticle(mass: 1, position: (0.97000436f, -0.24308753f), velocity: (0.4662036850f, 0.4323657300f));
        config.AddParticle(mass: 1, position: (-0.97000436f, 0.24308753f), velocity: (0.4662036850f, 0.4323657300f));
        config.AddParticle(mass: 1, position: (0, 0), velocity: (-0.93240737f, -0.86473146f));

        using (Simulation sim = config.BuildBeeman())
        {
            sim.RunToEnd();
        }
    }

    private static void RunDaisyChainGravitySystem()
    {
        var config = new SimulationConfig()
        {
            DeltaTime = 0.001f,
            MaxSimulationTime = 9,
            SaveEverySteps = 10,
            OutputFile = "gravitydaisychain-N{count}-{type}-{steps}steps.txt",
            ForceFunction = new ForceFunctions.Gravity(g: 1),
        };

        const int N = 5;
        const float R = 1;
        const float S = 1.2f;

        for (int i = 0; i < N; i++)
        {
            config.AddParticle(
                mass: 1,
                position: (R * MathF.Cos(2 * MathF.PI * i / N), R * MathF.Sin(2 * MathF.PI * i / N)),
                velocity: (-S * MathF.Sin(2 * MathF.PI * i / N), S * MathF.Cos(2 * MathF.PI * i / N))
            );
        }

        using (Simulation sim = config.BuildVerlet())
        {
            sim.RunToEnd();
        }
    }
}