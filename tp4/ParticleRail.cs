using Silk.NET.Maths;

namespace tp4;

public abstract class ParticleRail
{
    public abstract Vector2D<double> getPosition(double time);
    public abstract Vector2D<double> getVelocity(double time);
}

public static class ParticleRails
{
    public class OscillatorRail : ParticleRail
    {
        public double A { get; }
        public double W { get; }

        public OscillatorRail(double a, double w)
        {
            A = a;
            W = w;
        }

        public override Vector2D<double> getPosition(double time)
        {
            return new Vector2D<double>(0, A * Math.Cos(W * time));
        }

        public override Vector2D<double> getVelocity(double time)
        {
            return new Vector2D<double>(0, -A * W * Math.Sin(W * time));
        }
    }
}