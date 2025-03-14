using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SimulationBase;
using tp1.Simulate;

namespace tp1;

public static class ProgramGraphs
{
    public static void RunSimulations()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Console.WriteLine("Starting up...");

        int[] MValues = { 1, 2, 3, 4, 8, 12, 16, 20, 24 };
        const int TRIES = 100;
        string outputFile = "simulation_results.txt";

        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            writer.WriteLine("N\tM\tTime(ms)");

            foreach (int m in MValues)
            {
                int count = 0;
                List<double> elapseds = new();
                for (int i = 0; i < TRIES; i++)
                {
                    var config = SimulationConfig.FromFile("Examples/Static100.txt");
                    count = config.Particles.Count;
                    config.M = m;

                    var simulation = new Simulation(config);

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    simulation.Initialize();
                    stopwatch.Stop();

                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    elapseds.Add(elapsedTime.TotalMilliseconds);
                }

                double average = elapseds.Average();
                double stdev = elapseds.StandardDeviation();
                Console.WriteLine($"Simulation with M={m} completed in average {average} ms.");
                writer.WriteLine($"{count}\t{m}\t{average}\t{stdev}");
            }
        }

        Console.WriteLine("Results saved in simulation_results.txt");
        Console.WriteLine("Goodbye!");
    }
}