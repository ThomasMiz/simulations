using System.Numerics;

namespace tp4;

public abstract class ParticleRail
{
    public abstract Vector2 getPosition(float time);
    public abstract Vector2 getVelocity(float time);
}

public static class ParticleRails
{
    public class OscillatorRail : ParticleRail
    {
        public float A { get; }
        public float W { get; }

        public OscillatorRail(float a, float w)
        {
            A = a;
            W = w;
        }

        public override Vector2 getPosition(float time)
        {
            return new Vector2(0, A * MathF.Cos(W * time));
        }

        public override Vector2 getVelocity(float time)
        {
            return new Vector2(0, -A * W * MathF.Sin(W * time));
        }
    }
}