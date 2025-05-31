using tp5.Particles;

namespace tp5.Integration;

public interface IntegrationMethod
{
    /// <summary>
    /// This integration method's name.
    /// </summary>
    String Name { get; }

    /// <summary>
    /// Called whenever a particle is added to the simulation.
    /// </summary>
    void InitializeParticle(Particle particle, double deltaTime);

    /// <summary>
    /// Calculates the next position and velocity for each particle, updating their NextPosition and NextVelocity.
    /// </summary>
    void Step(IEnumerable<Particle> particles, double deltaTime);
}