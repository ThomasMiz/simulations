using System.Numerics;

namespace tp4;

public class VerletSimulation : Simulation
{
    public const string TypeName = "verlet";

    // Verlet requires the positions of the next step to calculate the velocity of the current step, so to make the
    // output always be position+velocity together, the position calculations are always one step ahead.
    private Vector2[] nextStatePositions;

    public VerletSimulation(string? outputFile, SimulationConfig? config) : base(TypeName, 2, outputFile, config)
    {
        nextStatePositions = new Vector2[CurrentState.Length];
    }

    protected override void InitializeImpl()
    {
        ParticleState[] currentState = CurrentState;
        ParticleState[] prevDummyState = new ParticleState[currentState.Length];

        for (int i = 0; i < currentState.Length; i++)
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
                continue;
            }

            float mass = Consts[i].Mass;
            Vector2 force = ForceFunction.Apply(Consts, currentState, i);
            prevDummyState[i] = new ParticleState
            {
                Position = currentState[i].Position - DeltaTime * currentState[i].Velocity + Math2.Square(DeltaTime) / (2 * mass) * force,
                Velocity = currentState[i].Velocity + (DeltaTime / mass) * force,
            };

            // Calculate the positions for the next state
            nextStatePositions[i] = 2 * currentState[i].Position - prevDummyState[i].Position + Math2.Square(DeltaTime) / mass * force;
        }

        AddOlderState(prevDummyState); // Add it last, as the previous state.
    }

    protected override void StepImpl(ParticleState[] nextState)
    {
        ParticleState[] currentState = CurrentState; // State at time = t
        // nextState: State at time = (t + dt) (what we're calculating)

        for (int i = 0; i < nextState.Length; i++)
        {
            if (Rails[i] != null)
            {
                nextState[i] = new ParticleState
                {
                    Position = Rails[i]!.getPosition(SecondsElapsed),
                    Velocity = Rails[i]!.getVelocity(SecondsElapsed)
                };
                continue;
            }

            nextState[i].Position = nextStatePositions[i];

            Vector2 force = ForceFunction.Apply(Consts, currentState, i);
            nextState[i].Velocity = currentState[i].Velocity + force * DeltaTime / Consts[i].Mass; // Predicted velocity, will be overwritten later
        }

        for (int i = 0; i < nextState.Length; i++)
        {
            if (Rails[i] != null)
            {
                continue;
            }

            // Calculate r(t + 2dt), which in the next iteration will be r(t + dt)
            Vector2 force = ForceFunction.Apply(Consts, nextState, i); // Force F(t + dt)
            nextStatePositions[i] = 2 * nextStatePositions[i] - currentState[i].Position + Math2.Square(DeltaTime) / Consts[i].Mass * force;

            // For cases such as the "Oscilador Amortiguado", where the force F(t) depends on the velocity v(t), the
            // Verlet equations are recursive: to calculate F(t) we need v(t), which needs r(t + dt), which needs F(t).
            // To solve this, we use the most recently available positions and velocities: F(t) = f(r(t), v(t-1))
            nextState[i].Velocity = (nextStatePositions[i] - currentState[i].Position) / (2 * DeltaTime);
        }
    }
}