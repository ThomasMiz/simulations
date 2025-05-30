using System.Numerics;
using AetherPhysics.Dynamics;

namespace tp5.Particles;

public class ConstantForceAvoidingParticle : CircularSensingParticle
{
    public Vector2 Force { get; set; }

    public ConstantForceAvoidingParticle(float radius, float sensingRadius, Vector2 force) : base(radius, sensingRadius)
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
        Vector2 force = Force;
        
        foreach (Body sensedBody in SensedBodies)
        {
            float forceMax = 2;
            
            Vector2 diff = Body.Position - sensedBody.Position;
            float distance = diff.Length();
            Vector2 direction = diff / distance;
            
            force += direction * (forceMax * distance / SensingRadius);
        }
        
        Body.ApplyForce(force);
    }

    public override void PostUpdate(float deltaTime, double elapsed)
    {
    }
}