using System.Globalization;

namespace tp2;

public class ProbabilityGraphProgram
{
    public static void MakeGraphs()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Console.WriteLine("Starting up simulation...");

        List<float> probabilityValues = [0.01f, 0.04f, 0.07f, 0.85f, 0.1f, 0.15f, 0.2f, 0.5f, 0.75f, 0.9f];

        Parallel.ForEach(probabilityValues, (probability =>
        {
            var config = new SimulationConfig()
            {
                GridFile = "data/semilla.txt",
                Probability = probability,
                RandomSeed = 1234,
                MaxSteps = 10000,
                ConsensusEpsilon = 0,
                StationaryEpsilon = 0,
                OutputFile = $"output-{probability}.txt",
                ConsensoFile = $"consenso-{probability}.txt",
                ClusterStatsFile = $"clusterstats-{probability}.txt",
            };

            using var simulation = config.Build();

            simulation.Run();
        }));


        Console.WriteLine("Goodbye :-)");
    }
}