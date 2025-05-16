using System.Globalization;

namespace tp4;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        //RunSimpleSystems(0.01f);
        //RunComplexSystem(2 * Math.PI, 102.3, 0.001);
        //RunEightLoopGravitySystem();
        //RunDaisyChainGravitySystem();
        //RunAllDeltaTimes();
        RunComplexSystemSweep();

        //List<Action> runs = [RunSimpleSystems, RunComplexSystem, RunEightLoopGravitySystem, RunDaisyChainGravitySystem];
        //Parallel.ForEach(runs, a => a());

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

        var config = new SimulationConfig
        {
            DeltaTime = deltaTime,
            MaxSimulationTime = 8,
            OutputFile = $"output-simple-{{type}}-{{steps}}steps-dt{deltaTime:e0}.txt",
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

    public static void RunComplexSystemSweep()
    {
        //double[] ks = new[] {  3e2, 1e3, 3e3, 1e4 }; // pasa a kg/s²
        double[] ks = new[] { 1.023e2 };
        //double[] omegas = new[] { 0.5*Math.PI, Math.PI, 1.5*Math.PI, 2 * Math.PI, 2.5*Math.PI, 3*Math.PI, 3.5*Math.PI, 4*Math.PI, 4.5*Math.PI, 5*Math.PI, 6 * Math.PI, 7*Math.PI, 9*Math.PI, 12 * Math.PI };
        double[] omegas = new[] { 2.0 };
        //double[] deltaTimes = { 1e-4, 1e-3, 1e-2};
        double[] deltaTimes = { 1e-4 };

        Parallel.ForEach(ks, k =>
        {
            Parallel.ForEach(omegas, omega =>
            {
                Parallel.ForEach(deltaTimes, dt =>
                {
                    RunComplexSystem(omega, k, dt);
                });
            });
        });
    }



    private static void RunComplexSystem(double wp, double kp, double dt)
    {
        // All mass units are in kg, all time units are in seconds
        const double m = 0.00021;
        double k = kp;
        const double gamma = 0.0003;
        const double A = 0.01;
        const double l0 = 0.001;
        double w = wp;
        const int N = 1000;
        var config = new SimulationConfig
        {
            DeltaTime = dt,
            MaxSimulationTime = 8,
            SaveEverySteps = 10,
            OutputFile = $"complex-k{k:0.##e0}-w{w}.txt",
            ForceFunction = new ForceFunctions.OsciladoresAcoplados(k: k, y: gamma)
        };

        config.AddRailedParticle(mass: m, rail: new ParticleRails.OscillatorRail(a: A, w: w));
        for (int i = 1; i < N; i++)
            config.AddParticle(mass: m, position: (l0 * i, 0), velocity: (0, 0));

        List<Action> runs =
        [
            /*() =>
            {
                using Simulation verlet = config.BuildVerlet();
                verlet.RunToEnd();
            },
            () =>
            {
                using Simulation beeman = config.BuildBeeman();
                beeman.RunToEnd();
            },*/
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