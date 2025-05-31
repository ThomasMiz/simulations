using System.Numerics;

namespace tp5.Particles;

public class ConstantForceParticle : CircularParticle
{
    public Vector2 Force { get; set; }

    public ConstantForceParticle(float radius, Vector2 force) : base(radius)
    {
        Force = force;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        Body.LinearDamping = 2;
        if (Force.X < 0) Body.Rotation = MathF.PI;
    }

    public override void PreUpdate(float deltaTime, double elapsed)
    {
        Body.ApplyForce(Force);
    }

    public override void PostUpdate(float deltaTime, double elapsed)
    {
    }
}