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

    protected Vector2D<double>? GetSpawningPosition()
    {
        // Try to generate a random position that is not taken
        const int randomCheckLimit = 5;
        for (int i = 0; i < randomCheckLimit; i++)
        {
            Vector2D<double> position = new()
            {
                X = random.NextDouble(SpawnArea.Left, SpawnArea.Right),
                Y = random.NextDouble(SpawnArea.Bottom, SpawnArea.Top),
            };

            if (!Simulation.ExistsAnyWithinDistance(position, 0.25f, 0.25f))
                return position;
        }

        // Force check that there are no possible spawn places before aborting the sim
        for (double x = SpawnArea.Left; x <= SpawnArea.Right; x += 0.1)
        for (double y = SpawnArea.Bottom; y <= SpawnArea.Top; y += 0.1)
        {
            Vector2D<double> position = new(x, y);

            if (!Simulation.ExistsAnyWithinDistance(position, 0.25f, 0.25f))
                return position;
        }

        Simulation.NoSpace = true;
        return null;
    }
}