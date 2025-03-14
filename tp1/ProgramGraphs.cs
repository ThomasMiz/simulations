using System;
using System.Diagnostics;
using System.IO;
using tp1.Simulate;

namespace tp1;

public static class ProgramGraphs
{
    public static void RunSimulations()
    {
        Console.WriteLine("Starting up...");

        int[] MValues = { 1, 2, 3, 4, 8, 12, 16, 20, 24 };
        string outputFile = "simulation_results.txt";

        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            writer.WriteLine("N\tM\tTime(ms)");

            foreach (int m in MValues)
            {
                var config = SimulationConfig.FromFile("Examples/Static100.txt");
                config = SimulationConfig.ReadM(config, m);

                var simulation = new Simulation(config);

                Stopwatch stopwatch = Stopwatch.StartNew();
                simulation.Initialize();
                stopwatch.Stop();

                TimeSpan elapsedTime = stopwatch.Elapsed;

                writer.WriteLine($"{config.Particles.Count}\t{m}\t{elapsedTime.TotalMilliseconds}");
                Console.WriteLine($"Simulation with M={m} completed in {elapsedTime.TotalMilliseconds} ms.");
            }
        }

        Console.WriteLine("Results saved in simulation_results.txt");
        Console.WriteLine("Goodbye!");
    }
}