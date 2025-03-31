using System.Globalization;

namespace tp2;

public class ProgramB
{
    public static void MakeGraphs()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Console.WriteLine("Starting up simulation...");

        List<float> probabilityValues = [0.01f, 0.1f, 0.9f];

        Parallel.ForEach(probabilityValues, (probability =>
        {
            var config = new SimulationConfig()
            {
                GridFile = "data/semilla-50.txt",
                Probability = probability,
                RandomSeed = 1234,
                MaxSteps = 10000,
                ConsensusEpsilon = 0,
                StationaryEpsilon = 0,
                OutputFile = $"output-b-{probability}.txt",
                ConsensoFile = $"consenso-b-{probability}.txt",
                ClusterStatsFile = $"clusterstats-b-{probability}.txt",
            };

            using var simulation = config.Build();

            simulation.Run();
        }));


        Console.WriteLine("Goodbye :-)");
    }
}