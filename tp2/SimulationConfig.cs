namespace tp2;

public class SimulationConfig
{
    const float DefaultConsensusEpsilon = 0.04f;
    const uint DefaultContinueAfterConsensus = 0;

    public string? GridFile { get; set; } = null;

    public (int, int)? GridSize { get; set; } = null;

    public float? Probability { get; set; } = null;

    public uint? MaxSteps { get; set; } = null;
    public float ConsensusEpsilon { get; set; } = DefaultConsensusEpsilon;
    public uint ContinueAfterConsensus { get; set; } = DefaultContinueAfterConsensus;

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
        Random random = RandomSeed.HasValue ? new Random(RandomSeed.Value) : new Random();

        sbyte[,] grid;
        if (GridFile != null)
        {
            grid = loadGridFromFile(GridFile);
        }
        else if (GridSize.HasValue)
        {
            grid = new sbyte[GridSize.Value.Item1, GridSize.Value.Item2];
            byte[] randbytes = new byte[grid.GetLength(0) * grid.GetLength(1)];
            random.NextBytes(randbytes);

            int i = 0;
            for (int x = 0; x < grid.GetLength(0); x++)
            for (int y = 0; y < grid.GetLength(1); y++)
                grid[x, y] = randbytes[i++] < 128 ? (sbyte)(-1) : (sbyte)1;
        }
        else
        {
            throw new Exception("No grid file nor size specified");
        }

        return new Simulation(
            grid,
            Probability ?? throw new Exception("No probability specified"),
            MaxSteps,
            ConsensusEpsilon,
            ContinueAfterConsensus,
            random,
            OutputFile,
            ConsensoFile,
            ClusterStatsFile,
            IncludeClusterStats
        );
    }
}