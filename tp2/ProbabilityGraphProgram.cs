using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;

namespace tp2;

public class ProbabilityGraphProgram
{
    public static void MakeGraphs()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Console.WriteLine("Starting up simulation...");

        List<float> probabilityValues = [0.0f, 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.085f, 0.09f, 0.095f, 0.1f, 0.105f, 0.1f, 0.15f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f];
        //List<float> probabilityValues = [0.0f, 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.85f, 0.09f, 0.091f, 0.092f, 0.093f, 0.094f, 0.095f, 0.096f, 0.097f, 0.098f, 0.099f, 0.1f, 0.101f, 0.102f, 0.103f, 0.104f, 0.105f, 0.1f, 0.15f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f];
        //List<float> probabilityValues = Enumerable.Range(0, 200).Select(x => x * 0.001f).ToList();

        ConcurrentDictionary<float, float> consensusAverageMap = new();
        ConcurrentDictionary<float, float> susceptibilityAverageMap = new();

        Parallel.ForEach(probabilityValues, probability =>
        {
            var config = new SimulationConfig()
            {
                GridFile = "data/semilla.txt",
                Probability = probability,
                RandomSeed = 1234,
                MaxSteps = 20000,
                ConsensusEpsilon = 0,
                StationaryEpsilon = 0,
                OutputFile = $"output-{probability}.txt",
                ConsensoFile = $"consenso-{probability}.txt",
                ClusterStatsFile = $"clusterstats-{probability}.txt",
                IncludeClusterStats = false,
            };

            using var simulation = config.Build();
            simulation.Run();

            int skipSteps = (int)config.MaxSteps - 5000;
            consensusAverageMap[probability] = simulation.ConsensusHistory.Skip(skipSteps).Average();
            susceptibilityAverageMap[probability] = simulation.CalculateSusceptibility(skipSteps);
        });

        Console.WriteLine($"Done running simulations!");
        Console.WriteLine();

        var consensus = consensusAverageMap.ToImmutableSortedDictionary();
        var susceptibility = susceptibilityAverageMap.ToImmutableSortedDictionary();
        //foreach (var (key, value) in consensus)
        //    Console.WriteLine($"p={key} => consensus average = {value}");

        Console.WriteLine();

        //foreach (var (key, value) in susceptibility)
        //    Console.WriteLine($"p={key} => susceptibility = {value}");

        Console.WriteLine("Consensus & Susceptibility average plot:");
        Console.WriteLine("p = [" + string.Join(',', consensus.Keys) + "]");
        Console.WriteLine("consensus = [" + string.Join(',', consensus.Values) + "]");
        Console.WriteLine("susceptibility = [" + string.Join(',', susceptibility.Values) + "]");

        Console.WriteLine("Goodbye :-)");
    }
}