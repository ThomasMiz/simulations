using System.Numerics;
using AetherPhysics.Dynamics;

namespace tp5.Particles;

public class ConstantForceAvoidingParticle : CircularSensingParticle
{
    public Vector2 Force { get; set; }
    public float MaxEvasiveForce { get; set; }

    public ConstantForceAvoidingParticle(float radius, float sensingRadius, Vector2 force, float maxEvasiveForce) : base(radius, sensingRadius)
    {
        Force = force;
        MaxEvasiveForce = maxEvasiveForce;
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
            Vector2 diff = Body.Position - sensedBody.Position;
            float distance = diff.Length();
            Vector2 direction = diff / distance;

            Vector2 evasiveForce = direction * (MaxEvasiveForce * distance / SensingRadius);

            if (Body.LinearVelocity != Vector2.Zero && sensedBody.LinearVelocity != Vector2.Zero)
            {
                float dot = -Vector2.Dot(Vector2.Normalize(Body.LinearVelocity), Vector2.Normalize(sensedBody.LinearVelocity));
                evasiveForce *= new Vector2(Math.Max(dot, 0), dot);
            }

            force += evasiveForce;
        }

        Body.ApplyForce(force);
    }

    public override void PostUpdate(float deltaTime, double elapsed)
    {
    }
}