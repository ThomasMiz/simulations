using Silk.NET.Maths;
using tp5.Particles;

namespace tp5.Integration;

public class Gear5Integration : IntegrationMethod
{
    public string Name => "gear5";

    public void InitializeParticle(Particle particle, double deltaTime)
    {
        particle.Aux0 = particle.CalculateDerivative(0);
        particle.Aux1 = particle.CalculateDerivative(1);
        particle.Aux2 = particle.CalculateDerivative(2);
        particle.Aux3 = particle.CalculateDerivative(3);
        particle.Aux4 = particle.CalculateDerivative(4);
        particle.Aux5 = particle.CalculateDerivative(5);
    }

    public void Step(IEnumerable<Particle> particles, double deltaTime)
    {
        // Calculate all the predicted variables
        Parallel.ForEach(particles, particle =>
        {
            Vector2D<double> prevAux0 = particle.Aux0;
            Vector2D<double> prevAux1 = particle.Aux1;
            Vector2D<double> prevAux2 = particle.Aux2;
            Vector2D<double> prevAux3 = particle.Aux3;
            Vector2D<double> prevAux4 = particle.Aux4;
            Vector2D<double> prevAux5 = particle.Aux5;

            particle.Aux0 = prevAux0 + prevAux1 * deltaTime + prevAux2 * Math2.Square(deltaTime) / 2f
                            + prevAux3 * Math2.Cube(deltaTime) / 6f + prevAux4 * Math2.Pow4(deltaTime) / 24f
                            + prevAux5 * Math2.Pow5(deltaTime) / 120f;

            particle.Aux1 = prevAux1 + prevAux2 * deltaTime + prevAux3 * Math2.Square(deltaTime) / 2f
                            + prevAux4 * Math2.Cube(deltaTime) / 6f + prevAux5 * Math2.Pow4(deltaTime) / 24f;

            particle.Aux2 = prevAux2 + prevAux3 * deltaTime + prevAux4 * Math2.Square(deltaTime) / 2f
                            + prevAux5 * Math2.Cube(deltaTime) / 6f;

            particle.Aux3 = prevAux3 + prevAux4 * deltaTime + prevAux5 * Math2.Square(deltaTime) / 2f;

            particle.Aux4 = prevAux4 + prevAux5 * deltaTime;

            particle.Aux5 = prevAux5;

            // Used for the force calculations, will be overwritten in the next loop
            particle.Position = particle.Aux0;
            particle.Velocity = particle.Aux1;
        });

        Parallel.ForEach(particles, particle =>
        {
            GearConstants gearConstants = particle.IsForceVelocityDependant ? GearConstants.PositionAndVelocityDependentForces : GearConstants.PositionDependentForces;

            Vector2D<double> force = particle.CalculateForce();
            Vector2D<double> a = force / particle.Mass;

            Vector2D<double> deltaA = a - particle.Aux2;
            Vector2D<double> deltaR2 = deltaA * Math2.Square(deltaTime) / 2f;

            // Apply corrections
            particle.Aux0 += gearConstants.a0 * deltaR2;
            particle.Aux1 += gearConstants.a1 * deltaR2 / deltaTime;
            particle.Aux2 += gearConstants.a2 * deltaR2 * 2f / Math2.Square(deltaTime);
            particle.Aux3 += gearConstants.a3 * deltaR2 * 6f / Math2.Cube(deltaTime);
            particle.Aux4 += gearConstants.a4 * deltaR2 * 24f / Math2.Pow4(deltaTime);
            particle.Aux5 += gearConstants.a5 * deltaR2 * 120f / Math2.Pow5(deltaTime);

            particle.NextPosition = particle.Aux0;
            particle.NextVelocity = particle.Aux1;
        });
    }
}