using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Spawners;

public abstract class RateBasedParticleSpawner : RandomAreaParticleSpawner
{
    public double SpawnRate { get; set; }
    private readonly double spawnEvery;
    private double nextSpawnTime = 0;

    public RateBasedParticleSpawner(double spawnRate, Bounds spawnArea) : base(spawnArea)
    {
        SpawnRate = spawnRate;
        spawnEvery = 1.0 / SpawnRate;
    }

    public override void PreUpdate()
    {
        double elapsed = Simulation.SecondsElapsed;

        while (nextSpawnTime < elapsed)
        {
            Vector2D<double> position = GetSpawningPosition();
            Simulation.AddParticle(CreateParticle(position));
            nextSpawnTime += spawnEvery;
        }
    }

    protected abstract Particle CreateParticle(Vector2D<double> position);
}