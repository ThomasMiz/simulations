using System.Numerics;

namespace tp4;

/**
 * Receives the simulation's state and the index of a particle and returns the force to exert on the particle with said
 * index.
 * <param name="states">The states of the particles in the simulation.</param>
 * <param name="index">The index of the particle to calculate force for.</param>
 */
public delegate Vector2 ForceFunction(ParticleState[] states, int index);

public static class ForceFunctions
{
    public static ForceFunction OsciladorAmortiguado(float k, float y)
    {
        return (states, i) => new Vector2(-k * states[i].Position.X - y * states[i].Velocity.X, 0);
    }
}