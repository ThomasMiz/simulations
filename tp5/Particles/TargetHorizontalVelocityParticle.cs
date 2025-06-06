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

    public float Ky { get; set; } = 1;

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

    protected override Vector2D<double> CalculateForceImpl()
    {
        Vector2D<double> requiredAsceleration = new Vector2D<double>(TargetHorizontalVelocity - Velocity.X, -Velocity.Y) / Simulation.DeltaTime;
        Vector2D<double> absoluteAsceleration = Vector2D.Min(Vector2D.Abs(requiredAsceleration), new Vector2D<double>(Acceleration, -Velocity.Y * Ky));
        double ax = Math.CopySign(absoluteAsceleration.X, requiredAsceleration.X);
        double ay = Math.CopySign(absoluteAsceleration.Y, requiredAsceleration.Y);

        return new Vector2D<double>(ax, ay) * Mass;
    }
}