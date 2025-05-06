using Silk.NET.Maths;

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
    public abstract Vector2D<double> Apply(ParticleConsts[] consts, ParticleState[] states, int index);

    public abstract Vector2D<double> GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index);
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

        public override Vector2D<double> Apply(ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return new Vector2D<double>(-K * states[index].Position.X - Y * states[index].Velocity.X, 0);
        }

        public override Vector2D<double> GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index)
        {
            double m = consts[index].Mass;

            return derivative switch
            {
                0 => states[index].Position, // x
                1 => states[index].Velocity, // v
                2 => Apply(consts, states, index) / consts[index].Mass, // a
                // 3 => new Vector2(-K * states[index].Velocity.X - Y * Apply(states, index).X / consts[index].Mass, 0),
                // 3 => new Vector2(-K * GetDerivative(1, states, consts, index).X - Y * GetDerivative(2, states, consts, index).X, 0),
                int i when i >= 3 => new Vector2D<double>(
                    (-K * GetDerivative(i - 2, consts, states, index).X 
                     - Y * GetDerivative(i - 1, consts, states, index).X) / m,
                    0
                ),
                _ => Vector2D<double>.Zero
                
            };
        }
    }

    public class OsciladoresAcoplados : ForceFunction
    {
        public double K { get; }
        public double Y { get; }

        public OsciladoresAcoplados(double k, double y)
        {
            K = k;
            Y = y;
        }

        public override bool IsVelocityDependant => true;

        public override Vector2D<double> Apply(ParticleConsts[] consts, ParticleState[] states, int index)
        {
            double y = states[index].Position.Y;
            double yLeft = index == 0 ? 0 : states[index - 1].Position.Y;
            double yRight = index + 1 == states.Length ? 0 : states[index + 1].Position.Y;
            return new Vector2D<double>(0, -K * (y - yLeft) - K * (y - yRight) - Y * states[index].Velocity.Y);
        }

        public override Vector2D<double> GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return derivative switch
            {
                0 => states[index].Position,
                1 => states[index].Velocity,
                2 => Apply(consts, states, index) / consts[index].Mass,
                int i => Vector2D<double>.Zero
            };
        }
    }

    public class Gravity : ForceFunction
    {
        public const double DEFAULT_G = 6.693e-11;

        public double G { get; }
        public override bool IsVelocityDependant => false;

        public Gravity(double g = DEFAULT_G)
        {
            G = g;
        }

        public override Vector2D<double> Apply(ParticleConsts[] consts, ParticleState[] states, int index)
        {
            Vector2D<double> f = Vector2D<double>.Zero;
            for (int i = 0; i < states.Length; i++)
            {
                if (i == index)
                    continue;

                Vector2D<double> positionDifference = states[i].Position - states[index].Position;
                double distanceSquared = positionDifference.LengthSquared;
                Vector2D<double> directionUnit = Vector2D.Normalize(positionDifference);
                f += G * consts[index].Mass * consts[i].Mass / distanceSquared * directionUnit;
            }

            return f;
        }

        public override Vector2D<double> GetDerivative(int derivative, ParticleConsts[] consts, ParticleState[] states, int index)
        {
            return derivative switch
            {
                0 => states[index].Position,
                1 => states[index].Velocity,
                2 => Apply(consts, states, index) / consts[index].Mass,
                _ => Vector2D<double>.Zero
            };
        }
    }
}