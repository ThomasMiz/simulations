using Silk.NET.Maths;

namespace tp5.Particles;

public class TargetHorizontalVelocityParticle : Particle
{
    public override string Name => IsLeftToRight ? "TargetHorizontalVelocityLeft" : "TargetHorizontalVelocityRight";
    public override bool IsForceVelocityDependant => true;

    public double TargetHorizontalVelocity { get; set; }

    public double Acceleration { get; set; }

    public double TargetX { get; set; }

    public bool IsLeftToRight => TargetHorizontalVelocity > 0;

    public float Ky { get; set; } = 5;

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
        Vector2D<double> requiredAsceleration = new Vector2D<double>(TargetHorizontalVelocity - Velocity.X, -Velocity.Y) / Simulation.DeltaTime;
        Vector2D<double> absoluteAscelerationX = Vector2D.Min(Vector2D.Abs(requiredAsceleration), new Vector2D<double>(Acceleration, -Velocity.Y * Ky));
        double ax = Math.CopySign(absoluteAscelerationX.X, requiredAsceleration.X);
        double ay = Math.CopySign(absoluteAscelerationX.Y, requiredAsceleration.Y);

        return new Vector2D<double>(ax, ay) * Mass;
    }
}