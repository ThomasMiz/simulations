using Silk.NET.Maths;

namespace tp4;

public class VerletSimulation : Simulation
{
    public const string TypeName = "verlet";

    // Verlet requires the positions of the next step to calculate the velocity of the current step, so to make the
    // output always be position+velocity together, the position calculations are always one step ahead.
    private readonly Vector2D<double>[] nextStatePositions;

    public VerletSimulation(string? outputFile, SimulationConfig? config) : base(TypeName, 2, outputFile, config)
    {
        nextStatePositions = new Vector2D<double>[CurrentState.Length];
    }

    protected override void InitializeImpl()
    {
        ParticleState[] currentState = CurrentState;
        ParticleState[] prevDummyState = new ParticleState[currentState.Length];

        Parallel.For(0, currentState.Length, i =>
        {
            // Create a "dummy" state previous to the real initial state using simple Euler interpolation
            // This is because the verlet algorithm needs the two previous states to calculate the next

            if (Rails[i] != null)
            {
                prevDummyState[i] = new ParticleState
                {
                    Position = Rails[i]!.getPosition(-DeltaTime),
                    Velocity = Rails[i]!.getVelocity(-DeltaTime)
                };
                nextStatePositions[i] = Rails[i]!.getPosition(DeltaTime);
                return;
            }

            double mass = Consts[i].Mass;
            Vector2D<double> force = ForceFunction.Apply(Consts, currentState, i);
            prevDummyState[i] = new ParticleState
            {
                Position = currentState[i].Position - DeltaTime * currentState[i].Velocity + Math2.Square(DeltaTime) / (2 * mass) * force,
                Velocity = currentState[i].Velocity - (DeltaTime / mass) * force
            };

            // Calculate the positions for the next state
            nextStatePositions[i] = 2 * currentState[i].Position - prevDummyState[i].Position + Math2.Square(DeltaTime) / mass * force;
        });

        AddOlderState(prevDummyState); // Add it last, as the previous state.
    }

    protected override void StepImpl(ParticleState[] nextState)
    {
        ParticleState[] currentState = CurrentState; // State at time = t
        // nextState: State at time = (t + dt) (what we're calculating)

        Parallel.For(0, nextState.Length, i =>
        {
            if (Rails[i] != null)
            {
                nextState[i] = new ParticleState
                {
                    Position = Rails[i]!.getPosition(SecondsElapsed),
                    Velocity = Rails[i]!.getVelocity(SecondsElapsed)
                };
                return;
            }

            nextState[i].Position = nextStatePositions[i];

            Vector2D<double> force = ForceFunction.Apply(Consts, currentState, i);
            nextState[i].Velocity = currentState[i].Velocity + force * DeltaTime / Consts[i].Mass; // Predicted velocity, will be overwritten later
        });

        Parallel.For(0, nextState.Length, i =>
        {
            if (Rails[i] != null) return;

            // For cases such as the "Oscilador Amortiguado", where the force F(t) depends on the velocity v(t), the
            // Verlet equations are recursive: to calculate F(t) we need v(t), which needs r(t + dt), which needs F(t).
            // To solve this, force calculations use the most recently available positions and predicted the velocities.

            // Calculate r(t + 2dt), which in the next iteration will be r(t + dt)
            Vector2D<double> force = ForceFunction.Apply(Consts, nextState, i); // Force F(t + dt)
            nextStatePositions[i] = 2 * nextStatePositions[i] - currentState[i].Position + Math2.Square(DeltaTime) / Consts[i].Mass * force;
            nextState[i].Velocity = (nextStatePositions[i] - currentState[i].Position) / (2 * DeltaTime);
        });
    }
}