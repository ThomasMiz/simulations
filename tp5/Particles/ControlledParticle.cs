using Silk.NET.Maths;

namespace tp5.Particles;

public class ControlledParticle : Particle
{
    public ForceFunction ForceFunction { get; init; }

    public override void OnInitialized()
    {
    }

    public override Vector2D<double> CalculateForce()
    {
        return ForceFunction.Apply(this);
    }

    public override Vector2D<double> CalculateDerivative(int derivative)
    {
        return ForceFunction.GetDerivative(derivative, this);
    }
}