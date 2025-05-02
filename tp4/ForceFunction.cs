using System.Numerics;

namespace tp4;

public abstract class ForceFunction
{
    public abstract bool IsVelocityDependant { get; }

    /**
     * Receives the simulation's state and the index of a particle and returns the force to exert on the particle with said
     * index.
     * <param name="states">The states of the particles in the simulation.</param>
     * <param name="index">The index of the particle to calculate force for.</param>
     */
    public abstract Vector2 Apply(ParticleConsts[] consts, ParticleState[] states, int index);

    public abstract Vector2 GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index);
}

public static class ForceFunctions
{
    public class OsciladorAmortiguado : ForceFunction
    {
        public float K { get; }
        public float Y { get; }

        public OsciladorAmortiguado(float k, float y)
        {
            K = k;
            Y = y;
        }

        public override bool IsVelocityDependant => true;

        public override Vector2 Apply(ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return new Vector2(-K * states[index].Position.X - Y * states[index].Velocity.X, 0);
        }

        public override Vector2 GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return derivative switch
            {
                0 => states[index].Position, // x
                1 => states[index].Velocity, // v
                2 => Apply(consts, states, index) / consts[index].Mass, // a
                // 3 => new Vector2(-K * states[index].Velocity.X - Y * Apply(states, index).X / consts[index].Mass, 0),
                // 3 => new Vector2(-K * GetDerivative(1, states, consts, index).X - Y * GetDerivative(2, states, consts, index).X, 0),
                int i => new Vector2(-K * GetDerivative(i - 2, consts, states, index).X - Y * GetDerivative(i - 1, consts, states, index).X, 0),
            };
        }
    }

    public class OsciladoresAcoplados : ForceFunction
    {
        public float K { get; }
        public float Y { get; }

        public OsciladoresAcoplados(float k, float y)
        {
            K = k;
            Y = y;
        }

        public override bool IsVelocityDependant => true;

        public override Vector2 Apply(ParticleConsts[] consts, ParticleState[] states, int index)
        {
            float y = states[index].Position.Y;
            float yLeft = index == 0 ? 0 : states[index - 1].Position.Y;
            float yRight = index + 1 == states.Length ? 0 : states[index + 1].Position.Y;
            return new Vector2(0, -K * (y - yLeft) - K * (y - yRight) - Y * states[index].Velocity.Y);
        }

        public override Vector2 GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return derivative switch
            {
                0 => states[index].Position,
                1 => states[index].Velocity,
                2 => Apply(consts, states, index) / consts[index].Mass,
                int i => new Vector2(0, 0),
            };
        }
    }

    public class Gravity : ForceFunction
    {
        public const float DEFAULT_G = 6.693e-11f;

        public float G { get; }
        public override bool IsVelocityDependant => false;

        public Gravity(float g = DEFAULT_G)
        {
            G = g;
        }

        public override Vector2 Apply(ParticleConsts[] consts, ParticleState[] states, int index)
        {
            Vector2 f = Vector2.Zero;
            for (int i = 0; i < states.Length; i++)
            {
                if (i == index)
                    continue;

                Vector2 positionDifference = states[i].Position - states[index].Position;
                float distanceSquared = positionDifference.LengthSquared();
                Vector2 directionUnit = Vector2.Normalize(positionDifference);
                f += G * consts[index].Mass * consts[i].Mass / distanceSquared * directionUnit;
            }

            return f;
        }

        public override Vector2 GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return derivative switch
            {
                0 => states[index].Position,
                1 => states[index].Velocity,
                2 => Apply(consts, states, index) / consts[index].Mass,
                _ => Vector2.Zero,
            };
        }
    }
}