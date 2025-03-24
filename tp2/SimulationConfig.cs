namespace tp2;

public class SimulationConfig
{
    const float DefaultConsensusEpsilon = 0.04f;
    const float DefaultStationaryEpsilon = 0.001f;
    const uint DefaultStationaryWindowSize = 10;

    private sbyte[,]? grid;
    private float? probability;

    private uint? maxSteps;
    private float consensusEpsilon = DefaultConsensusEpsilon;
    private float stationaryEpsilon = DefaultStationaryEpsilon;
    private uint stationaryWindowSize = DefaultStationaryWindowSize;

    private Random? random;

    private string? outputFile;
    private string? consensoFile;

    public SimulationConfig WithSeedFile(string file)
    {
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

        return this;
    }

    public SimulationConfig WithProbability(float probability)
    {
        if (probability < 0 || probability > 1)
            throw new Exception("Probability must be between 0 and 1");

        this.probability = probability;
        return this;
    }

    public SimulationConfig WithMaxSteps(uint? maxSteps)
    {
        this.maxSteps = maxSteps;
        return this;
    }

    public SimulationConfig WithConsensusEpsilon(float epsilon)
    {
        if (epsilon < 0)
            throw new Exception("Consensus epsilon must be greater than 0");

        consensusEpsilon = epsilon;
        return this;
    }

    public SimulationConfig WithStationaryEpsilon(float epsilon)
    {
        if (epsilon < 0)
            throw new Exception("Stationary epsilon must be greater than 0");

        stationaryEpsilon = epsilon;
        return this;
    }

    public SimulationConfig WithStationaryWindowSize(uint stationaryWindowSize)
    {
        if (stationaryWindowSize <= 0)
            throw new Exception("Stationary window size must be greater than 0");

        this.stationaryWindowSize = stationaryWindowSize;
        return this;
    }

    public SimulationConfig WithRandomSeed(int seed)
    {
        random = new Random(seed);
        return this;
    }

    public SimulationConfig WithOutputFile(string? file)
    {
        outputFile = file;
        return this;
    }

    public SimulationConfig WithConsensoFile(string? file)
    {
        consensoFile = file;
        return this;
    }

    public Simulation Build()
    {
        return new Simulation(
            grid ?? throw new Exception("No grid specified"),
            probability ?? throw new Exception("No probability specified"),
            maxSteps,
            consensusEpsilon,
            stationaryEpsilon,
            stationaryWindowSize,
            random ?? new Random(),
            outputFile,
            consensoFile
        );
    }
}