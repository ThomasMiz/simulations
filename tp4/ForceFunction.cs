using System.Numerics;

namespace tp4;

/**
 * Receives the simulation's state and the index of a particle and returns the force to exert on the particle with said
 * index.
 * <param name="positionStates">The state with the positions to use for the calculations. Velocities will never be read from here.</param>
 * <param name="velocityStates">The state with the velocities to use for the calculations. Positions will never be read from here.</param>
 * <param name="index">The index of the particle to calculate force for.</param>
 */
public delegate Vector2 ForceFunction(ParticleState[] positionStates, ParticleState[] velocityStates, int index);

public static class ForceFunctions
{
    public static ForceFunction OsciladorAmortiguado(float k, float y)
    {
        return (poss, vels, i) => new Vector2(-k * poss[i].Position.X - y * vels[i].Velocity.X, 0);
    }
}