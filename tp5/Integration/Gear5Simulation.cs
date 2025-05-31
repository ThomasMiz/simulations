using Silk.NET.Maths;

namespace tp5.Integration;

public class Gear5Simulation : Simulation
{
    public const string TypeName = "gear5";

    private readonly GearConstants gearConstants;

    public Gear5Simulation(string? outputFile, SimulationConfig config) : base(TypeName, outputFile, config)
    {
        gearConstants = ForceFunction.IsVelocityDependant ? GearConstants.PositionAndVelocityDependentForces : GearConstants.PositionDependentForces;
    }

    protected override void InitializeImpl()
    {
        Parallel.ForEach(Particles, particle =>
        {
            particle.Aux0 = ForceFunction.GetDerivative(0, particle);
            particle.Aux1 = ForceFunction.GetDerivative(1, particle);
            particle.Aux2 = ForceFunction.GetDerivative(2, particle);
            particle.Aux3 = ForceFunction.GetDerivative(3, particle);
            particle.Aux4 = ForceFunction.GetDerivative(4, particle);
            particle.Aux5 = ForceFunction.GetDerivative(5, particle);
        });
    }

    protected override void StepImpl()
    {
        // Calculate all the predicted variables
        Parallel.ForEach(Particles, particle =>
        {
            Vector2D<double> prevAux0 = particle.Aux0;
            Vector2D<double> prevAux1 = particle.Aux1;
            Vector2D<double> prevAux2 = particle.Aux2;
            Vector2D<double> prevAux3 = particle.Aux3;
            Vector2D<double> prevAux4 = particle.Aux4;
            Vector2D<double> prevAux5 = particle.Aux5;

            particle.Aux0 = prevAux0 + prevAux1 * DeltaTime + prevAux2 * Math2.Square(DeltaTime) / 2f
                            + prevAux3 * Math2.Cube(DeltaTime) / 6f + prevAux4 * Math2.Pow4(DeltaTime) / 24f
                            + prevAux5 * Math2.Pow5(DeltaTime) / 120f;

            particle.Aux1 = prevAux1 + prevAux2 * DeltaTime + prevAux3 * Math2.Square(DeltaTime) / 2f
                            + prevAux4 * Math2.Cube(DeltaTime) / 6f + prevAux5 * Math2.Pow4(DeltaTime) / 24f;

            particle.Aux2 = prevAux2 + prevAux3 * DeltaTime + prevAux4 * Math2.Square(DeltaTime) / 2f
                            + prevAux5 * Math2.Cube(DeltaTime) / 6f;

            particle.Aux3 = prevAux3 + prevAux4 * DeltaTime + prevAux5 * Math2.Square(DeltaTime) / 2f;

            particle.Aux4 = prevAux4 + prevAux5 * DeltaTime;

            particle.Aux5 = prevAux5;

            // Used for the force calculations, will be overwritten in the next loop
            particle.Position = particle.Aux0;
            particle.Velocity = particle.Aux1;
        });

        Parallel.ForEach(Particles, particle =>
        {
            Vector2D<double> force = ForceFunction.Apply(particle);
            Vector2D<double> a = force / particle.Mass;

            Vector2D<double> deltaA = a - particle.Aux2;
            Vector2D<double> deltaR2 = deltaA * Math2.Square(DeltaTime) / 2f;

            // Apply corrections
            particle.Aux0 += gearConstants.a0 * deltaR2;
            particle.Aux1 += gearConstants.a1 * deltaR2 / DeltaTime;
            particle.Aux2 += gearConstants.a2 * deltaR2 * 2f / Math2.Square(DeltaTime);
            particle.Aux3 += gearConstants.a3 * deltaR2 * 6f / Math2.Cube(DeltaTime);
            particle.Aux4 += gearConstants.a4 * deltaR2 * 24f / Math2.Pow4(DeltaTime);
            particle.Aux5 += gearConstants.a5 * deltaR2 * 120f / Math2.Pow5(DeltaTime);

            particle.NextPosition = particle.Aux0;
            particle.NextVelocity = particle.Aux1;
        });
    }
}