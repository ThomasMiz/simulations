using Silk.NET.Maths;

namespace tp5.Particles;

public class OsciladorAmortiguadoParticle : Particle
{
    public double K { get; set; }
    public double Y { get; set; }

    public override string Name => "OsciladorAmortiguado";
    public override bool IsForceVelocityDependant => true;

    protected override void OnInitializedImpl()
    {
    }

    public override void PostUpdate()
    {
        if (Math.Abs(Position.X) < 0.1 && Math.Abs(Velocity.X) < 0.1)
            Remove();
    }

    protected override Vector2D<double> CalculateForceImpl()
    {
        return new Vector2D<double>(-K * Position.X - Y * Velocity.X, 0);
    }

    public override Vector2D<double> CalculateDerivative(int derivative)
    {
        return derivative switch
        {
            0 => Position, // x
            1 => Velocity, // v
            2 => CalculateForce() / Mass, // a
            int i when i >= 3 => new Vector2D<double>(
                (-K * CalculateDerivative(i - 2).X - Y * CalculateDerivative(i - 1).X) / Mass,
                0
            ),
            _ => Vector2D<double>.Zero
        };
    }
}