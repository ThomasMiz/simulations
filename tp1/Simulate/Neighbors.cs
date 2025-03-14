using System;
using System.Collections.Generic;
using System.Numerics;

namespace tp1.Simulate;

public class Neighbors
{
    public Vector2 SimulationSize { get; private set; }
    public Vector2 BinSize { get; set; }
    public bool LoopsAround { get; set; }

    private readonly ICollection<Particle> particles;

    private List<Particle>?[,]? grid;
    private int gridWidth, gridHeight;

    public Neighbors(Vector2 simulationSize, Vector2 binSize, bool loopsAround, ICollection<Particle> particles)
    {
        SimulationSize = simulationSize;
        BinSize = binSize;
        LoopsAround = loopsAround;
        this.particles = particles;
        Recalculate();
    }

    public void Recalculate()
    {
        // Clear the grid and ensure it has enough size
        gridWidth = (int)MathF.Ceiling(SimulationSize.X / BinSize.X);
        gridHeight = (int)MathF.Ceiling(SimulationSize.Y / BinSize.Y);
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
            int minCellX = (int)((p.Position.X - p.Radius) / BinSize.X);
            int minCellY = (int)((p.Position.Y - p.Radius) / BinSize.Y);
            int maxCellX = (int)((p.Position.X + p.Radius) / BinSize.X);
            int maxCellY = (int)((p.Position.Y + p.Radius) / BinSize.Y);

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
                    Vector2 v = Vector2.Clamp(p.Position, new Vector2(x, y) * BinSize, new Vector2(x + 1, y + 1) * BinSize);
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
        int minCellX = (int)((position.X - radius) / BinSize.X);
        int minCellY = (int)((position.Y - radius) / BinSize.Y);
        int maxCellX = (int)((position.X + radius) / BinSize.X);
        int maxCellY = (int)((position.Y + radius) / BinSize.Y);

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
        Dictionary<Particle, HashSet<Particle>> result = new(particles.Count);
        foreach (Particle particle in particles)
        {
            result.Add(particle, new());
        }

        foreach (Particle particle in particles)
        {
            HashSet<Particle> currentResult = result[particle];

            int centerCellX = (int)(particle.Position.X / BinSize.X);
            int centerCellY = (int)(particle.Position.Y / BinSize.Y);

            int minCellX = (int)((particle.Position.X - radius) / BinSize.X);
            int minCellY = (int)((particle.Position.Y - radius) / BinSize.Y);
            int maxCellX = (int)((particle.Position.X + radius) / BinSize.X);
            int maxCellY = (int)((particle.Position.Y + radius) / BinSize.Y);

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

                    int y = (gridY + gridHeight) % gridHeight;
                    int x = (gridX + gridWidth) % gridWidth;

                    if (grid[x, y] == null) continue;

                    foreach (Particle candidate in grid[x, y])
                    {
                        if (ReferenceEquals(candidate, particle))
                        {
                            continue;
                        }

                        Vector2 v = particle.Position - candidate.Position;
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