namespace tp2;

public class SimulationConfig
{
    const float DefaultConsensusEpsilon = 0.04f;
    const float DefaultStationaryEpsilon = 0.001f;
    const uint DefaultStationaryWindowSize = 10;
    const uint DefaultContinueAfterStationary = 0;

    public string? GridFile { get; set; } = null;

    public float? Probability { get; set; } = null;

    public uint? MaxSteps { get; set; } = null;
    public float ConsensusEpsilon { get; set; } = DefaultConsensusEpsilon;
    public float StationaryEpsilon { get; set; } = DefaultStationaryEpsilon;
    public uint StationaryWindowSize { get; set; } = DefaultStationaryWindowSize;
    public uint ContinueAfterStationary { get; set; } = DefaultContinueAfterStationary;

    public int? RandomSeed { get; set; } = null;

    public string? OutputFile { get; set; } = null;
    public string? ConsensoFile { get; set; } = null;
    public string? ClusterStatsFile { get; set; } = null;
    
    public bool IncludeClusterStats { get; set; } = true;

    private static sbyte[,] loadGridFromFile(string file)
    {
        sbyte[,] grid = null;
        string[] lines = File.ReadAllLines(file);
        if (lines == null || lines.Length == 0)
            throw new Exception("Simulation seed file is empty");

        for (int y = 0; y < lines.Length; y++)
        {
            string[] split = lines[y].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (grid == null)
                grid = new sbyte[split.Length, lines.Length];
            else if (grid.GetLength(1) != split.Length)
                throw new Exception("All lines in the seed file must have the same amount of elements");

            for (int x = 0; x < split.Length; x++)
                grid[x, y] = sbyte.Parse(split[x]);
        }

        return grid;
    }

    public Simulation Build()
    {
        return new Simulation(
            loadGridFromFile(GridFile ?? throw new Exception("No grid file specified")),
            Probability ?? throw new Exception("No probability specified"),
            MaxSteps,
            ConsensusEpsilon,
            StationaryEpsilon,
            StationaryWindowSize,
            ContinueAfterStationary,
            RandomSeed.HasValue ? new Random(RandomSeed.Value) : new Random(),
            OutputFile,
            ConsensoFile,
            ClusterStatsFile,
            IncludeClusterStats
        );
    }
}