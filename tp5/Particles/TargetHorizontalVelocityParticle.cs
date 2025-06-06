using Silk.NET.Maths;

namespace tp5.Particles;

public class TargetHorizontalVelocityParticle : Particle
{
    public override string Name => IsLeftToRight ? "TargetHorizontalVelocityLeft" : "TargetHorizontalVelocityRight";
    public override bool IsForceVelocityDependant => true;

    public double TargetHorizontalVelocity { get; set; }

    public double Tao { get; set; } = 0.5;

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
        return new Vector2D<double>((TargetHorizontalVelocity - Velocity.X) / Tao, -Velocity.Y * Ky) * Mass;
    }
}