using AetherPhysics.Dynamics;

namespace tp5.Particles;

public abstract class Particle
{
    public Body Body { get; private set; }

    public LinkedListNode<Particle> Node { get; set; } = null;

    public void Initialize(Body body)
    {
        Body = body;
        body.Tag = this;
        body.FixedRotation = true;
        body.BodyType = BodyType.Dynamic;
        
        OnInitialized();
        
        foreach (Fixture fixture in body.FixtureList)
        {
            fixture.Friction = 0;
            fixture.Restitution = 0;
        }
    }
    
    protected abstract void OnInitialized();
    
    public abstract void PreUpdate(float deltaTime, double elapsed);
    
    public abstract void PostUpdate(float deltaTime, double elapsed);
}