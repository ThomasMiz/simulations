using Silk.NET.Maths;

namespace tp5.Integration;

public class BeemanSimulation : Simulation
{
    public const string TypeName = "beeman";

    // The particle's "Aux0" and "Aux1" are used to store the previous position and velocity respectively.
    // "Aux2", "Aux3" and "Aux4" are respectively the force in the previous, current and predicted state.

    public BeemanSimulation(string? outputFile, SimulationConfig config) : base(TypeName, outputFile, config)
    {
    }

    protected override void InitializeImpl()
    {
        Parallel.ForEach(Particles, particle =>
        {
            Vector2D<double> a = ForceFunction.Apply(particle) / particle.Mass;
            particle.Aux0 = particle.Position - DeltaTime * particle.Velocity + 0.5 * a * Math2.Square(DeltaTime);
            particle.Aux1 = particle.Velocity - a * DeltaTime;
            particle.Aux2 = a;
        });
    }

    protected override void StepImpl()
    {
        Parallel.ForEach(Particles, particle =>
        {
            Vector2D<double> a_t = ForceFunction.Apply(particle) / particle.Mass;
            Vector2D<double> a_tm1 = particle.Aux2;

            Vector2D<double> x_next = particle.Position
                                      + particle.Velocity * DeltaTime
                                      + (2.0 / 3.0 * a_t - 1.0 / 6.0 * a_tm1) * Math2.Square(DeltaTime);

            Vector2D<double> v_predict = particle.Velocity
                                         + (3.0 / 2.0 * a_t - 1.0 / 2.0 * a_tm1) * DeltaTime;

            particle.Aux5 = particle.Velocity;
            particle.Position = x_next;
            particle.Velocity = v_predict;
            particle.Aux4 = a_t;
        });

        // Now calculate the actual new positions and velocities
        Parallel.ForEach(Particles, particle =>
        {
            Vector2D<double> a_t = particle.Aux4;
            Vector2D<double> a_tm1 = particle.Aux2;
            Vector2D<double> a_tp1 = ForceFunction.Apply(particle) / particle.Mass;

            particle.NextPosition = particle.Position;
            particle.NextVelocity = particle.Aux5 + (1.0 / 3.0 * a_tp1 + 5.0 / 6.0 * a_t - 1.0 / 6.0 * a_tm1) * DeltaTime;
            particle.Aux2 = a_t;
        });
    }
}