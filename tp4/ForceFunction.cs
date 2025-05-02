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
    public abstract Vector2 Apply(ParticleState[] states, int index);

    public abstract Vector2 GetDerivative(int derivative, ParticleState[] states, ParticleConsts[] consts, int index);
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

        public override Vector2 Apply(ParticleState[] states, int index)
        {
            return new Vector2(-K * states[index].Position.X - Y * states[index].Velocity.X, 0);
        }

        public override Vector2 GetDerivative(int derivative, ParticleState[] states, ParticleConsts[] consts, int index)
        {
            return derivative switch
            {
                0 => states[index].Position, // x
                1 => states[index].Velocity, // v
                2 => Apply(states, index) / consts[index].Mass, // a
                // 3 => new Vector2(-K * states[index].Velocity.X - Y * Apply(states, index).X / consts[index].Mass, 0),
                // 3 => new Vector2(-K * GetDerivative(1, states, consts, index).X - Y * GetDerivative(2, states, consts, index).X, 0),
                int i => new Vector2(-K * GetDerivative(i - 2, states, consts, index).X - Y * GetDerivative(i - 1, states, consts, index).X, 0),
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

        public override Vector2 Apply(ParticleState[] states, int index)
        {
            float y = states[index].Position.Y;
            float yLeft = index == 0 ? 0 : states[index - 1].Position.Y;
            float yRight = index + 1 == states.Length ? 0 : states[index + 1].Position.Y;
            return new Vector2(0, -K * (y - yLeft) - K * (y - yRight) - Y * states[index].Velocity.Y);
        }

        public override Vector2 GetDerivative(int derivative, ParticleState[] states, ParticleConsts[] consts, int index)
        {
            return derivative switch
            {
                0 => states[index].Position,
                1 => states[index].Velocity,
                2 => Apply(states, index) / consts[index].Mass,
                int i => new Vector2(0, 0),
            };
        }
    }
}