namespace tp2;

public class Simulation : IDisposable
{
    public uint Steps { get; private set; } = 0;
    public uint? ConsensusReachStep { get; private set; } = null;
    public bool ConsensusReached => ConsensusReachStep != null;
    public uint RemainingConsensusSteps { get; private set; }

    public sbyte[,] Grid { get; }
    public float Probability { get; }
    public uint? MaxSteps { get; }
    public float ConsensusEpsilon { get; }

    public bool IncludeClusterStats { get; }

    private readonly ClusterCalculator? clusterCalculator;

    private readonly List<float> consensusHistory = new();
    private readonly List<ClusterStats> clusterStatsHistory = new();

    public IReadOnlyList<float> ConsensusHistory => consensusHistory;
    public IReadOnlyList<ClusterStats> ClusterStatsHistory => clusterStatsHistory;

    private readonly Random random;

    private readonly StreamWriter? outputStream;
    private readonly StreamWriter? consensoStream;
    private readonly StreamWriter? clusterStatsStream;

    public Simulation(sbyte[,] grid, float probability, uint? maxSteps, float consensusEpsilon, uint continueAfterConsensus, Random random, string? outputFile, string? consensoFile, string? clusterStatsFile, bool includeClusterStats)
    {
        Grid = grid;
        Probability = probability;
        MaxSteps = maxSteps;
        ConsensusEpsilon = consensusEpsilon;

        RemainingConsensusSteps = continueAfterConsensus;
        IncludeClusterStats = includeClusterStats;

        clusterCalculator = includeClusterStats ? new(grid) : null;

        this.random = random;

        outputStream = outputFile == null ? null : File.CreateText(outputFile);
        consensoStream = consensoFile == null ? null : File.CreateText(consensoFile);
        clusterStatsStream = clusterStatsFile == null ? null : File.CreateText(clusterStatsFile);
    }

    private void monteCarloStep()
    {
        int gridWidth = Grid.GetLength(0);
        int gridHeight = Grid.GetLength(1);

        sbyte getWrapping(int x, int y) => Grid[(x + gridWidth) % gridWidth, (y + gridHeight) % gridHeight];
        sbyte getMajority(int x, int y) => (sbyte)Math.Sign(getWrapping(x, y - 1) + getWrapping(x - 1, y) + getWrapping(x, y) + getWrapping(x + 1, y) + getWrapping(x, y + 1));

        for (int i = gridWidth * gridHeight; i != 0; i--)
        {
            int x = random.Next(gridWidth);
            int y = random.Next(gridHeight);

            sbyte majority = getMajority(x, y);

            int randomSign = Math.Sign(random.NextSingle() - Probability);
            majority *= (sbyte)randomSign;

            Grid[x, y] = majority;
        }
    }

    public void Run()
    {
        float m = Math.Abs(Grid.Cast<sbyte>().Select(s => (int)s).Sum()) / (float)(Grid.GetLength(0) * Grid.GetLength(1));
        consensusHistory.Add(m);

        ClusterStats? clusterStats = clusterCalculator?.Calculate();
        if (clusterStats != null) clusterStatsHistory.Add(clusterStats.Value);

        SaveDataToFiles(m, clusterStats);

        while (!ConsensusReached || RemainingConsensusSteps > 0)
        {
            monteCarloStep();
            int totalSum = Grid.Cast<sbyte>().Select(s => (int)s).Sum();

            m = Math.Abs(totalSum) / (float)(Grid.GetLength(0) * Grid.GetLength(1));
            consensusHistory.Add(m);

            clusterStats = clusterCalculator?.Calculate();
            if (clusterStats != null) clusterStatsHistory.Add(clusterStats.Value);

            SaveDataToFiles(m, clusterStats);

            Steps++;
            if (ConsensusReached) RemainingConsensusSteps--;

            if (Steps % 1000 == 0) Console.WriteLine("Ran {0} steps...", Steps);

            if (!ConsensusReached && m >= 1 - ConsensusEpsilon)
            {
                ConsensusReachStep = Steps;
                Console.WriteLine("Consensus reached after {0} steps", Steps);
            }

            if (MaxSteps.HasValue && Steps >= MaxSteps)
            {
                Console.WriteLine("Stopping simulation; max steps reached");
                break;
            }
        }

        Console.WriteLine("Simulation ended after {0} steps", Steps);
    }

    private void SaveDataToFiles(float m, in ClusterStats? clusterStats)
    {
        for (int y = 0; y < Grid.GetLength(1); y++)
        {
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                if (x != 0 || y != 0) outputStream?.Write(' ');
                outputStream?.Write(Grid[x, y]);
            }
        }

        outputStream?.WriteLine();

        if (consensoStream != null)
        {
            consensoStream.Write(Steps);
            consensoStream.Write(',');
            consensoStream.WriteLine(m);
        }

        if (clusterStatsStream != null && clusterStats != null)
        {
            clusterStatsStream.Write(Steps);
            clusterStatsStream.Write(',');
            clusterStatsStream.Write(clusterStats.Value.ClusterCount);
            clusterStatsStream.Write(',');
            clusterStatsStream.Write(clusterStats.Value.MinClusterSize);
            clusterStatsStream.Write(',');
            clusterStatsStream.Write(clusterStats.Value.MaxClusterSize);
            clusterStatsStream.Write(',');
            clusterStatsStream.WriteLine(clusterStats.Value.AverageClusterSize);
        }
    }

    public float CalculateSusceptibility()
    {
        if (!ConsensusReached)
            throw new Exception("Susceptibility cannot be calculated before a stationary state is reached");

        return CalculateSusceptibility((int)ConsensusReachStep);
    }

    public float CalculateSusceptibility(int fromStep, int? toStep = null)
    {
        int toStep2 = toStep ?? consensusHistory.Count;

        var sublist = consensusHistory[fromStep..toStep2];
        float mSquaredAverage = sublist.Select(m => m * m).Average();
        float mAverageSquared = Math2.Square(sublist.Average());
        int cellCount = Grid.GetLength(0) * Grid.GetLength(1);
        return cellCount * (mSquaredAverage - mAverageSquared);
    }

    public void Dispose()
    {
        outputStream?.Dispose();
        consensoStream?.Dispose();
        clusterStatsStream?.Dispose();
    }
}