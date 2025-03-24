namespace tp2;

public class Simulation : IDisposable
{
    public int Steps { get; private set; } = 0;
    public sbyte[,] Grid { get; }
    public float Probability { get; }
    public uint? MaxSteps { get; }
    public float Epsilon { get; }
    public uint StationaryWindowSize { get; }

    private Queue<float> consensusHistoryQueue;

    private readonly Random random;

    private readonly StreamWriter? outputStream;
    private readonly StreamWriter? consensoStream;

    public Simulation(sbyte[,] grid, float probability, uint? maxSteps, float epsilon, uint stationaryWindowSize, Random random, string? outputFile, string? consensoFile)
    {
        Grid = grid;
        Probability = probability;
        MaxSteps = maxSteps;
        Epsilon = epsilon;
        StationaryWindowSize = stationaryWindowSize;

        consensusHistoryQueue = new Queue<float>((int)stationaryWindowSize);

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
        while (true)
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

            if (consensusHistoryQueue.Count == StationaryWindowSize) consensusHistoryQueue.Dequeue();
            consensusHistoryQueue.Enqueue(m);

            if (consensusHistoryQueue.Count == StationaryWindowSize && consensusHistoryQueue.Max() - consensusHistoryQueue.Min() < Epsilon)
            {
                Console.WriteLine("Consensus reached after {0} steps", Steps);
                break;
            }

            if (Steps % 500 == 0)
            {
                Console.WriteLine("Ran {0} steps...", Steps);
            }

            if (Steps >= MaxSteps)
            {
                Console.WriteLine("Stopping simulation; max steps reached");
                break;
            }
        }
    }

    public void Dispose()
    {
        outputStream.Dispose();
        consensoStream.Dispose();
    }

    public static SimulationConfig Builder() => new();
}