using tp2;
using System.Globalization;

public static class Program
{
    public static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Console.WriteLine("Starting up simulation...");

        var config = new SimulationConfig()
        {
            GridFile = "data/semilla-50.txt",
            Probability = 0.02f,
            RandomSeed = 1234,
            MaxSteps = 50000,
            StationaryWindowSize = 100,
            ConsensusEpsilon = 0,
            StationaryEpsilon = 0,
            ContinueAfterStationary = 1000,
            OutputFile = "output.txt",
            ConsensoFile = "consenso.txt",
            ClusterStatsFile = "clusterstats.txt",
        };

        using var simulation = config.Build();

        simulation.Run();

        if (simulation.StationaryReached)
        {
            Console.WriteLine("Susceptibility is {0}", simulation.CalculateSusceptibility());
        }

        Console.WriteLine("goodbye :-)");
    }
}