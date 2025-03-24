namespace tp2;

public class ClusterCalculator
{
    private readonly sbyte[,] grid;
    private readonly int gridWidth;
    private readonly int gridHeight;

    private bool[,] visited;
    private List<int> clusterSizes;
    private Stack<(int, int)> dfsStack;

    public ClusterCalculator(sbyte[,] grid)
    {
        this.grid = grid;
        gridWidth = grid.GetLength(0);
        gridHeight = grid.GetLength(1);
    }

    private int visitClusterReturnSize(int startX, int startY)
    {
        // Reveal the entire cluster using Depth-First search
        dfsStack.Clear();
        int clusterValue = grid[startX, startY];

        void addIfRelevant(int x, int y)
        {
            x = (x + gridWidth) % gridWidth;
            y = (y + gridHeight) % gridHeight;
            if (!visited[x, y] && grid[x, y] == clusterValue)
                dfsStack.Push((x, y));
        }

        int size = 0;
        dfsStack.Push((startX, startY));
        while (dfsStack.TryPop(out (int, int) cell))
        {
            if (visited[cell.Item1, cell.Item2]) continue;

            size++;
            visited[cell.Item1, cell.Item2] = true;
            addIfRelevant(cell.Item1, cell.Item2 - 1);
            addIfRelevant(cell.Item1, cell.Item2 + 1);
            addIfRelevant(cell.Item1 - 1, cell.Item2);
            addIfRelevant(cell.Item1 + 1, cell.Item2);
        }

        return size;
    }

    public ClusterStats Calculate()
    {
        visited ??= new bool[gridHeight, gridWidth];
        clusterSizes ??= new();
        dfsStack ??= new();

        Array.Clear(visited);
        clusterSizes.Clear();

        for (int y = 0; y < gridHeight; y++)
        for (int x = 0; x < gridWidth; x++)
            if (!visited[x, y])
                clusterSizes.Add(visitClusterReturnSize(x, y));

        return new ClusterStats()
        {
            ClusterCount = clusterSizes.Count,
            MinClusterSize = clusterSizes.Min(),
            MaxClusterSize = clusterSizes.Max(),
            AverageClusterSize = (float)clusterSizes.Average(),
        };
    }
}