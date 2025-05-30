using AetherPhysics.Dynamics;

namespace tp5.Particles;

public abstract class CircularParticle : Particle
{
    public float Radius { get; }
    
    public Fixture BodyFixture { get; private set; }

    public CircularParticle(float radius)
    {
        Radius = radius;
    }

    protected override void OnInitialized()
    {
        BodyFixture = Body.CreateCircle(Radius, 1f);
    }
}