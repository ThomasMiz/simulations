using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Spawners;

public class TestyParticleSpawner : RateBasedParticleSpawner
{
    public TestyParticleSpawner(double spawnRate) : base(spawnRate, new Bounds(minX: -1, minY: -1, maxX: 1, maxY: 1))
    {
    }

    protected override Particle CreateParticle(Vector2D<double> position)
    {
        return new OsciladorAmortiguadoParticle()
        {
            Mass = 70,
            Radius = 0.25,
            Position = position,
            Velocity = new Vector2D<double>(-1 * 100.0 / (2 * 70), 0),
            K = 10000,
            Y = 100,
        };
    }
}