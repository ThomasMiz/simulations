namespace tp2;

public class Simulation : IDisposable
{
    public uint Steps { get; private set; } = 0;
    public uint? ConsensusReachStep { get; private set; } = null;
    public bool ConsensusReached => ConsensusReachStep != null;
    public uint? StationaryReachStep { get; private set; } = null;
    public bool StationaryReached => StationaryReachStep != null;
    public uint RemainingStationarySteps { get; private set; }

    public sbyte[,] Grid { get; }
    public float Probability { get; }
    public uint? MaxSteps { get; }
    public float ConsensusEpsilon { get; }
    public float StationaryEpsilon { get; }
    public uint StationaryWindowSize { get; }

    private List<float> consensusHistory = new();

    public IReadOnlyList<float> ConsensusHistory => consensusHistory;

    private readonly Random random;

    private readonly StreamWriter? outputStream;
    private readonly StreamWriter? consensoStream;

    public Simulation(sbyte[,] grid, float probability, uint? maxSteps, float consensusEpsilon, float stationaryEpsilon, uint stationaryWindowSize, uint continueAfterStationary, Random random, string? outputFile, string? consensoFile)
    {
        Grid = grid;
        Probability = probability;
        MaxSteps = maxSteps;
        ConsensusEpsilon = consensusEpsilon;
        StationaryEpsilon = stationaryEpsilon;
        StationaryWindowSize = stationaryWindowSize;

        RemainingStationarySteps = continueAfterStationary;

        this.random = random;

        outputStream = outputFile == null ? null : File.CreateText(outputFile);
        consensoStream = consensoFile == null ? null : File.CreateText(consensoFile);
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
        while (!StationaryReached || RemainingStationarySteps > 0)
        {
            monteCarloStep();
            int totalSum = 0;
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                for (int x = 0; x < Grid.GetLength(0); x++)
                {
                    if (x != 0 || y != 0) outputStream?.Write(' ');
                    outputStream?.Write(Grid[x, y]);
                    totalSum += Grid[x, y];
                }
            }

            outputStream?.WriteLine();

            float m = Math.Abs(totalSum) / (float)(Grid.GetLength(0) * Grid.GetLength(1));

            if (consensoStream != null)
            {
                consensoStream.Write(Steps);
                consensoStream.Write(',');
                consensoStream.WriteLine(m);
            }

            Steps++;
            if (StationaryReached) RemainingStationarySteps--;

            if (Steps % 1000 == 0) Console.WriteLine("Ran {0} steps...", Steps);

            consensusHistory.Add(m);

            if (!ConsensusReached && m >= 1 - ConsensusEpsilon)
            {
                ConsensusReachStep = Steps;
                StationaryReachStep ??= Steps;
                Console.WriteLine("Consensus reached after {0} steps", Steps);
            }

            if (!StationaryReached && consensusHistory.Count >= StationaryWindowSize)
            {
                var window = consensusHistory.TakeLast((int)StationaryWindowSize);
                if (window.Max() - window.Min() < StationaryEpsilon)
                {
                    StationaryReachStep = Steps;
                    Console.WriteLine("Stationary state reached after {0} steps", Steps);
                }
            }

            if (MaxSteps.HasValue && Steps >= MaxSteps)
            {
                Console.WriteLine("Stopping simulation; max steps reached");
                break;
            }
        }

        Console.WriteLine("Simulation ended after {0} steps", Steps);
    }

    public float CalculateSusceptibility()
    {
        if (!StationaryReached)
            throw new Exception("Susceptibility cannot be calculated before a stationary state is reached");

        float mSquaredAverage = consensusHistory.Skip((int)StationaryReachStep).Select(m => m * m).Average();
        float mAverageSquared = Math2.Square(consensusHistory.Skip((int)StationaryReachStep).Average());
        int cellCount = Grid.GetLength(0) * Grid.GetLength(1);
        return cellCount * (mSquaredAverage - mAverageSquared);
    }

    public void Dispose()
    {
        outputStream.Dispose();
        consensoStream.Dispose();
    }
}