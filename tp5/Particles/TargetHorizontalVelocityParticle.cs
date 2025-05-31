using Silk.NET.Maths;

namespace tp5.Particles;

public class TargetHorizontalVelocityParticle : Particle
{
    public override string Name => IsLeftToRight ? "TargetHorizontalVelocityLeft" : "TargetHorizontalVelocityRight";
    public override bool IsForceVelocityDependant => false;

    public double TargetHorizontalVelocity { get; set; }

    public double Acceleration { get; set; }

    public double TargetX { get; set; }

    public bool IsLeftToRight => TargetHorizontalVelocity > 0;

    protected override void OnInitializedImpl()
    {
    }

    public override void PostUpdate()
    {
        if ((Position.X > TargetX) == IsLeftToRight)
        {
            Remove();
        }
    }

    public override Vector2D<double> CalculateForce()
    {
        double requiredAsceleration = (TargetHorizontalVelocity - Velocity.X) / Simulation.DeltaTime;
        double absoluteAscelerationX = Math.Min(Math.Abs(requiredAsceleration), Acceleration);
        double fx = Math.CopySign(absoluteAscelerationX, requiredAsceleration) * Mass;

        return new Vector2D<double>(fx, 0);
    }
}