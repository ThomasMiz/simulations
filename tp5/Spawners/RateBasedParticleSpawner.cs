using Silk.NET.Maths;
using tp5.Particles;
using TrippyGL.Utils;

namespace tp5.Spawners;

public abstract class RateBasedParticleSpawner : ParticleSpawner
{
    public double SpawnRate { get; set; }
    private readonly double spawnEvery;
    private double nextSpawnTime = 0;

    public Bounds SpawnArea { get; set; }
    private Random random = new();

    public RateBasedParticleSpawner(double spawnRate, Bounds spawnArea)
    {
        SpawnRate = spawnRate;
        spawnEvery = 1.0 / SpawnRate;
        SpawnArea = spawnArea;
    }

    protected override void OnInitializedImpl()
    {
    }

    public override void PreUpdate()
    {
        double elapsed = Simulation.SecondsElapsed;

        while (nextSpawnTime < elapsed)
        {
            Vector2D<double> position = new()
            {
                X = random.NextDouble(SpawnArea.Left, SpawnArea.Right),
                Y = random.NextDouble(SpawnArea.Bottom, SpawnArea.Top),
            };

            Simulation.AddParticle(CreateParticle(position));
            nextSpawnTime += spawnEvery;
        }
    }

    protected abstract Particle CreateParticle(Vector2D<double> position);
}