using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Spawners;

public delegate Particle ParticleCreator(Vector2D<double> position);

public class GenericRateParticleSpawner : RateBasedParticleSpawner
{
    public ParticleCreator ParticleCreator { get; set; }

    public GenericRateParticleSpawner(double spawnRate, Bounds spawnArea, ParticleCreator particleCreator) : base(spawnRate, spawnArea)
    {
        ParticleCreator = particleCreator;
    }

    protected override void OnInitializedImpl()
    {
    }

    protected override Particle CreateParticle(Vector2D<double> position)
    {
        return ParticleCreator(position);
    }
}