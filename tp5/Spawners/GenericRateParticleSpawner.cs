using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Spawners;

public delegate Particle ParticleCreator(Vector2D<double> position);

public class GenericRateParticleSpawner : RateBasedParticleSpawner
{
    public ParticleCreator ParticleCreator { get; set; }

    public GenericRateParticleSpawner(double spawnRate, Bounds spawnArea, ParticleCreator particleCreator, uint? maxParticles = null) : base(spawnRate, spawnArea)
    {
        ParticleCreator = particleCreator;
        MaxParticles = maxParticles;
    }

    protected override void OnInitializedImpl()
    {
    }

    protected override Particle CreateParticle(Vector2D<double> position)
    {
        return ParticleCreator(position);
    }
}