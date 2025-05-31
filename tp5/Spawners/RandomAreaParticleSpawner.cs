using Silk.NET.Maths;
using TrippyGL.Utils;

namespace tp5.Spawners;

public abstract class RandomAreaParticleSpawner : ParticleSpawner
{
    public Bounds SpawnArea { get; set; }
    private Random random = new();

    public RandomAreaParticleSpawner(Bounds spawnArea)
    {
        SpawnArea = spawnArea;
    }

    protected Vector2D<double> GetSpawningPosition()
    {
        // TODO: Check that position is not already occupied, try find one that isn't
        return new()
        {
            X = random.NextDouble(SpawnArea.Left, SpawnArea.Right),
            Y = random.NextDouble(SpawnArea.Bottom, SpawnArea.Top),
        };
    }
}