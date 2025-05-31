using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Integration;

public class BeemanIntegration : IntegrationMethod
{
    public string Name => "beeman";

    public void InitializeParticle(Particle particle, double deltaTime)
    {
        Vector2D<double> a = particle.CalculateForce() / particle.Mass;
        particle.Aux0 = particle.Position - deltaTime * particle.Velocity + 0.5 * a * Math2.Square(deltaTime);
        particle.Aux1 = particle.Velocity - a * deltaTime;
        particle.Aux2 = a;
    }

    public void Step(IEnumerable<Particle> particles, double deltaTime)
    {
        Parallel.ForEach(particles, particle =>
        {
            Vector2D<double> a_t = particle.CalculateForce() / particle.Mass;
            Vector2D<double> a_tm1 = particle.Aux2;

            Vector2D<double> x_next = particle.Position
                                      + particle.Velocity * deltaTime
                                      + (2.0 / 3.0 * a_t - 1.0 / 6.0 * a_tm1) * Math2.Square(deltaTime);

            Vector2D<double> v_predict = particle.Velocity
                                         + (3.0 / 2.0 * a_t - 1.0 / 2.0 * a_tm1) * deltaTime;

            particle.Aux5 = particle.Velocity;
            particle.Position = x_next;
            particle.Velocity = v_predict;
            particle.Aux4 = a_t;
        });

        // Now calculate the actual new positions and velocities
        Parallel.ForEach(particles, particle =>
        {
            Vector2D<double> a_t = particle.Aux4;
            Vector2D<double> a_tm1 = particle.Aux2;
            Vector2D<double> a_tp1 = particle.CalculateForce() / particle.Mass;

            particle.NextPosition = particle.Position;
            particle.NextVelocity = particle.Aux5 + (1.0 / 3.0 * a_tp1 + 5.0 / 6.0 * a_t - 1.0 / 6.0 * a_tm1) * deltaTime;
            particle.Aux2 = a_t;
        });
    }
}