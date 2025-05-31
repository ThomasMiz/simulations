using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Integration;

public class TaylorIntegration : IntegrationMethod
{
    public string Name => "taylor";
    
    public void InitializeParticle(Particle particle, double deltaTime)
    {
        
    }

    public void Step(IEnumerable<Particle> particles, double deltaTime)
    {
        Parallel.ForEach(particles, particle =>
        {
            Vector2D<double> a = particle.CalculateForce() / particle.Mass;
            particle.NextPosition = particle.Position + particle.Velocity * deltaTime + a * 0.5 * Math2.Square(deltaTime);
            particle.NextVelocity = particle.Velocity + a * deltaTime;
        });
    }
}