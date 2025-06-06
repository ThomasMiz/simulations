using Silk.NET.Maths;
using tp5.Particles;

namespace tp5;

public class NeighborsFinder
{
    public Vector2D<double> SimulationSize { get; private set; }
    public Vector2D<int> BinCount { get; set; }
    public Vector2D<double> BinSize => SimulationSize / BinCount.As<double>();

    private readonly IReadOnlyCollection<Particle> particles;

    private List<Particle>?[,]? grid;
    private int gridWidth, gridHeight;

    public NeighborsFinder(Vector2D<double> simulationSize, Vector2D<int> binCount, IReadOnlyCollection<Particle> particles)
    {
        SimulationSize = simulationSize;
        BinCount = binCount;
        this.particles = particles;
        Recalculate();
    }

    public void Recalculate()
    {
        // Clear the grid and ensure it has enough size
        Vector2D<double> binSize = BinSize;
        gridWidth = (int)Math.Ceiling(SimulationSize.X / binSize.X);
        gridHeight = (int)Math.Ceiling(SimulationSize.Y / binSize.Y);
        if (grid == null || grid.GetLength(0) < gridWidth || grid.GetLength(1) < gridHeight)
        {
            grid = new List<Particle>[gridWidth, gridHeight];
        }
        else
        {
            for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                grid[x, y]?.Clear();
            }
        }

        // Iterate through all the particles and add each one to the cell it's in
        foreach (Particle particle in particles)
        {
            int cellX = Math.Clamp((int)(particle.Position.X / binSize.X), 0, gridWidth - 1);
            int cellY = Math.Clamp((int)(particle.Position.Y / binSize.Y), 0, gridHeight - 1);
            grid[cellX, cellY] ??= new List<Particle>();
            grid[cellX, cellY].Add(particle);
        }
    }

    public void ManualAddParticle(Particle particle)
    {
        Vector2D<double> binSize = BinSize;
        int cellX = (int)(particle.Position.X / binSize.X);
        int cellY = (int)(particle.Position.Y / binSize.Y);
        grid[cellX, cellY] ??= new List<Particle>();
        grid[cellX, cellY].Add(particle);
    }

    public void ManualRemoveParticle(Particle particle)
    {
        Vector2D<double> binSize = BinSize;
        int cellX = (int)(particle.Position.X / binSize.X);
        int cellY = (int)(particle.Position.Y / binSize.Y);
        grid[cellX, cellY]?.Remove(particle);
    }

    public bool ExistsAnyAtPoint(Vector2D<double> point, double particleRadius)
    {
        Vector2D<double> binSize = BinSize;

        int cellX = Math.Max((int)((point.X) / binSize.X), 0);
        int cellY = Math.Max((int)((point.Y) / binSize.Y), 0);

        return grid?[cellX, cellY]?.Any(p =>
        {
            Vector2D<double> v = point - p.Position;
            double maxDistance = particleRadius + p.Radius + particleRadius;
            return v.LengthSquared <= maxDistance * maxDistance;
        }) ?? false;
    }

    public bool ExistsAnyWithinDistance(in Vector2D<double> position, double particleRadius, double distance)
    {
        Vector2D<double> binSize = BinSize;
        double particleRadius2 = particleRadius * 2;

        int minCellX = Math.Max((int)((position.X - distance - particleRadius2) / binSize.X), 0);
        int minCellY = Math.Max((int)((position.Y - distance - particleRadius2) / binSize.Y), 0);
        int maxCellX = Math.Min((int)((position.X + distance + particleRadius2) / binSize.X), gridWidth - 1);
        int maxCellY = Math.Min((int)((position.Y + distance + particleRadius2) / binSize.Y), gridHeight - 1);

        for (int gridX = minCellX; gridX <= maxCellX; gridX++)
        {
            int x = (gridX + gridWidth) % gridWidth;
            for (int gridY = minCellY; gridY <= maxCellY; gridY++)
            {
                int y = (gridY + gridHeight) % gridHeight;

                if (grid[x, y] == null) continue;

                foreach (Particle candidate in grid[x, y])
                {
                    Vector2D<double> v = position - candidate.Position;
                    double maxDistance = distance + candidate.Radius + particleRadius;
                    if (v.LengthSquared <= maxDistance * maxDistance)
                        return true;
                }
            }
        }

        return false;
    }

    public void FindWithinDistance(in Vector2D<double> position, double particleRadius, double distance, ICollection<Particle> result)
    {
        Vector2D<double> binSize = BinSize;
        double particleRadius2 = particleRadius * 2;

        int minCellX = Math.Max((int)((position.X - distance - particleRadius2) / binSize.X), 0);
        int minCellY = Math.Max((int)((position.Y - distance - particleRadius2) / binSize.Y), 0);
        int maxCellX = Math.Min((int)((position.X + distance + particleRadius2) / binSize.X), gridWidth - 1);
        int maxCellY = Math.Min((int)((position.Y + distance + particleRadius2) / binSize.Y), gridHeight - 1);

        for (int gridX = minCellX; gridX <= maxCellX; gridX++)
        {
            int x = (gridX + gridWidth) % gridWidth;
            for (int gridY = minCellY; gridY <= maxCellY; gridY++)
            {
                int y = (gridY + gridHeight) % gridHeight;

                if (grid[x, y] == null) continue;

                foreach (Particle candidate in grid[x, y])
                {
                    Vector2D<double> v = position - candidate.Position;
                    double maxDistance = distance + candidate.Radius + particleRadius;
                    if (v.LengthSquared <= maxDistance * maxDistance)
                    {
                        result.Add(candidate);
                    }
                }
            }
        }
    }

    public List<Particle> FindWithinDistance(in Vector2D<double> position, double particleRadius, double distance)
    {
        List<Particle> result = new();
        FindWithinDistance(position, particleRadius, distance, result);
        return result;
    }

    /*public Dictionary<Particle, HashSet<Particle>> FindAllNeighbors(double radius)
    {
        Vector2D<double> binSize = BinSize;
        Dictionary<Particle, HashSet<Particle>> result = new(particles.Count);
        foreach (Particle particle in particles)
        {
            result.Add(particle, new());
        }

        foreach (Particle particle in particles)
        {
            HashSet<Particle> currentResult = result[particle];

            int centerCellX = (int)(particle.Position.X / binSize.X);
            int centerCellY = (int)(particle.Position.Y / binSize.Y);

            int minCellX = (int)((particle.Position.X - radius) / binSize.X);
            int minCellY = (int)((particle.Position.Y - radius) / binSize.Y);
            int maxCellX = (int)((particle.Position.X + radius) / binSize.X);
            int maxCellY = (int)((particle.Position.Y + radius) / binSize.Y);

            if (!LoopsAround)
            {
                minCellX = Math.Max(0, minCellX);
                maxCellX = Math.Min(gridWidth - 1, maxCellX);
                minCellY = Math.Max(0, minCellY);
                maxCellY = Math.Min(gridHeight - 1, maxCellY);
            }

            for (int gridX = minCellX; gridX <= maxCellX; gridX++)
            {
                for (int gridY = minCellY; gridY <= maxCellY; gridY++)
                {
                    if (!(gridX > centerCellX || (gridX == centerCellX && gridY >= centerCellY)))
                    {
                        continue;
                    }

                    int x = (gridX + gridWidth) % gridWidth;
                    int y = (gridY + gridHeight) % gridHeight;

                    Vector2D<double> candidateOffset = new Vector2D<double>((gridX - x) / BinCount.X, (gridY - y) / BinCount.Y) * SimulationSize;

                    if (grid[x, y] == null) continue;

                    foreach (Particle candidate in grid[x, y])
                    {
                        if (ReferenceEquals(candidate, particle))
                        {
                            continue;
                        }

                        Vector2D<double> v = particle.Position - (candidate.Position + candidateOffset);
                        double maxRadius = radius + candidate.Radius;
                        if (v.LengthSquared <= maxRadius * maxRadius)
                        {
                            currentResult.Add(candidate);
                            result[candidate].Add(particle);
                        }
                    }
                }
            }
        }

        return result;
    }*/
}