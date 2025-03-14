using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Maths;

namespace tp1.Simulate;

public class Neighbors
{
    public Vector2 SimulationSize { get; private set; }
    public Vector2D<int> BinCount { get; set; }
    public Vector2 BinSize => SimulationSize / BinCount.As<float>().ToSystem();
    public bool LoopsAround { get; set; }

    private readonly ICollection<Particle> particles;

    private List<Particle>?[,]? grid;
    private int gridWidth, gridHeight;

    public Neighbors(Vector2 simulationSize, Vector2D<int> binCount, bool loopsAround, ICollection<Particle> particles)
    {
        SimulationSize = simulationSize;
        BinCount = binCount;
        LoopsAround = loopsAround;
        this.particles = particles;
        Recalculate();
    }

    public void Recalculate()
    {
        // Clear the grid and ensure it has enough size
        Vector2 binSize = BinSize;
        gridWidth = (int)MathF.Ceiling(SimulationSize.X / binSize.X);
        gridHeight = (int)MathF.Ceiling(SimulationSize.Y / binSize.Y);
        if (grid == null || grid.GetLength(0) < gridWidth || grid.GetLength(1) < gridHeight)
        {
            grid = new List<Particle>[gridWidth, gridHeight];
        }
        else
        {
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    grid[x, y]?.Clear();
                }
            }
        }

        // Iterate through all the particles and add each one to the cells it intersects
        foreach (Particle p in particles)
        {
            int minCellX = (int)((p.Position.X - p.Radius) / binSize.X);
            int minCellY = (int)((p.Position.Y - p.Radius) / binSize.Y);
            int maxCellX = (int)((p.Position.X + p.Radius) / binSize.X);
            int maxCellY = (int)((p.Position.Y + p.Radius) / binSize.Y);

            if (!LoopsAround)
            {
                minCellX = Math.Max(0, minCellX);
                maxCellX = Math.Min(gridWidth - 1, maxCellX);
                minCellY = Math.Max(0, minCellY);
                maxCellY = Math.Min(gridHeight - 1, maxCellY);
            }

            for (int gridX = minCellX; gridX <= maxCellX; gridX++)
            {
                int x = (gridX + gridWidth) % gridWidth;
                for (int gridY = minCellY; gridY <= maxCellY; gridY++)
                {
                    int y = (gridY + gridHeight) % gridHeight;
                    Vector2 v = Vector2.Clamp(p.Position, new Vector2(x, y) * binSize, new Vector2(x + 1, y + 1) * binSize);
                    Vector2 d = p.Position - v;
                    float ds = d.LengthSquared();

                    if (ds <= p.Radius * p.Radius)
                    {
                        grid[x, y] ??= [];
                        grid[x, y].Add(p);
                    }
                }
            }
        }
    }

    public HashSet<Particle> FindWithinRadius(Particle particle, float radius)
    {
        HashSet<Particle> result = FindWithinRadius(particle.Position, radius + particle.Radius);
        result.Remove(particle);
        return result;
    }

    public HashSet<Particle> FindWithinRadius(Vector2 position, float radius)
    {
        Vector2 binSize = BinSize;
        int minCellX = (int)((position.X - radius) / binSize.X);
        int minCellY = (int)((position.Y - radius) / binSize.Y);
        int maxCellX = (int)((position.X + radius) / binSize.X);
        int maxCellY = (int)((position.Y + radius) / binSize.Y);

        if (!LoopsAround)
        {
            minCellX = Math.Max(0, minCellX);
            maxCellX = Math.Min(gridWidth - 1, maxCellX);
            minCellY = Math.Max(0, minCellY);
            maxCellY = Math.Min(gridHeight - 1, maxCellY);
        }

        HashSet<Particle> result = [];
        for (int gridX = minCellX; gridX <= maxCellX; gridX++)
        {
            int x = (gridX + gridWidth) % gridWidth;
            for (int gridY = minCellY; gridY <= maxCellY; gridY++)
            {
                int y = (gridY + gridHeight) % gridHeight;

                if (grid[x, y] == null) continue;

                foreach (Particle candidate in grid[x, y])
                {
                    Vector2 v = position - candidate.Position;
                    float maxRadius = radius + candidate.Radius;
                    if (v.LengthSquared() <= maxRadius * maxRadius)
                    {
                        result.Add(candidate);
                    }
                }
            }
        }

        return result;
    }

    public Dictionary<Particle, HashSet<Particle>> FindAllNeighbors(float radius)
    {
        Vector2 binSize = BinSize;
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

                    Vector2 candidateOffset = new Vector2((gridX - x) / BinCount.X, (gridY - y) / BinCount.Y) * SimulationSize;

                    if (grid[x, y] == null) continue;

                    foreach (Particle candidate in grid[x, y])
                    {
                        if (ReferenceEquals(candidate, particle))
                        {
                            continue;
                        }

                        Vector2 v = particle.Position - (candidate.Position + candidateOffset);
                        float maxRadius = radius + candidate.Radius;
                        if (v.LengthSquared() <= maxRadius * maxRadius)
                        {
                            currentResult.Add(candidate);
                            result[candidate].Add(particle);
                        }
                    }
                }
            }
        }

        return result;
    }
}