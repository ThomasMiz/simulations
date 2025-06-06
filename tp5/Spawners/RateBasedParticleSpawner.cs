using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Spawners;

public abstract class RateBasedParticleSpawner : RandomAreaParticleSpawner
{
    public double SpawnRate { get; set; }
    public uint? MaxParticles { get; set; }
    public uint SpawnedParticleCount { get; private set; } = 0;

    private readonly double spawnEvery;
    private double nextSpawnTime = 0;

    public RateBasedParticleSpawner(double spawnRate, Bounds spawnArea) : base(spawnArea)
    {
        SpawnRate = spawnRate;
        spawnEvery = 1.0 / SpawnRate;
    }

    public override void PreUpdate()
    {
        if (IsDone) return;

        double elapsed = Simulation.SecondsElapsed;

        //while
        if (nextSpawnTime < elapsed)
        {
            Vector2D<double>? position = GetSpawningPosition();
            if (position != null)
            {
                Simulation.AddParticle(CreateParticle(position.Value));
                SpawnedParticleCount++;
                IsDone = MaxParticles.HasValue && SpawnedParticleCount >= MaxParticles;
            }

            nextSpawnTime += spawnEvery;
        }
    }

    protected abstract Particle CreateParticle(Vector2D<double> position);
}