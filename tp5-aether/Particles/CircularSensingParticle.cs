using AetherPhysics.Dynamics;
using AetherPhysics.Dynamics.Contacts;

namespace tp5.Particles;

public abstract class CircularSensingParticle : CircularParticle
{
    public float SensingRadius { get; }

    public Fixture SensingFixture { get; private set; }

    private HashSet<Body> _sensedBodies = new();

    public IReadOnlySet<Body> SensedBodies => _sensedBodies;

    public CircularSensingParticle(float radius, float sensingRadius) : base(radius)
    {
        SensingRadius = sensingRadius;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        SensingFixture = Body.CreateCircle(SensingRadius, 0f);
        SensingFixture.IsSensor = true;
        SensingFixture.OnCollision += OnCollision;
        SensingFixture.OnSeparation += OnSeparation;
    }

    private bool OnCollision(Fixture me, Fixture other, Contact contact)
    {
        if (other.IsSensor || other.Body.BodyType == BodyType.Static) return false;
        
        _sensedBodies.Add(other.Body);
        return true;
    }

    private void OnSeparation(Fixture me, Fixture other, Contact contact)
    {
        if (other.IsSensor || other.Body.BodyType == BodyType.Static) return;
        
        _sensedBodies.Remove(other.Body);
    }
}