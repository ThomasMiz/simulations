using Silk.NET.Maths;
using tp5.Particles;

namespace tp5;

public abstract class ForceFunction
{
    public abstract bool IsVelocityDependant { get; }

    /**
     * Returns the force to exert on the particle.
     */
    public abstract Vector2D<double> Apply(Particle particles);

    public abstract Vector2D<double> GetDerivative(int derivative, Particle particle);
}

public static class ForceFunctions
{
    public class OsciladorAmortiguado : ForceFunction
    {
        public double K { get; }
        public double Y { get; }

        public OsciladorAmortiguado(double k, double y)
        {
            K = k;
            Y = y;
        }

        public override bool IsVelocityDependant => true;

        public override Vector2D<double> Apply(Particle particle)
        {
            return new Vector2D<double>(-K * particle.Position.X - Y * particle.Velocity.X, 0);
        }

        public override Vector2D<double> GetDerivative(int derivative, Particle particle)
        {
            double m = particle.Mass;

            return derivative switch
            {
                0 => particle.Position, // x
                1 => particle.Velocity, // v
                2 => Apply(particle) / particle.Mass, // a
                int i when i >= 3 => new Vector2D<double>(
                    (-K * GetDerivative(i - 2, particle).X - Y * GetDerivative(i - 1, particle).X) / m,
                    0
                ),
                _ => Vector2D<double>.Zero
            };
        }
    }
}