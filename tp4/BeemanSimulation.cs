using System.Numerics;

namespace tp4;

public class BeemanSimulation : Simulation
{
    public const string TypeName = "beeman";

    // Verlet requires the positions of the next step to calculate the velocity of the current step, so to make the
    // output always be position+velocity together, the position calculations are always one step ahead.
    private ParticleState[] predictedStates;

    public BeemanSimulation(string? outputFile, SimulationConfig config) : base(TypeName, 3, outputFile, config)
    {
        predictedStates = new ParticleState[CurrentState.Length];
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
                continue;
            }

            float mass = Consts[i].Mass;
            Vector2 force = ForceFunction.Apply(currentState, i);
            prevDummyState[i] = new ParticleState
            {
                Position = currentState[i].Position - DeltaTime * currentState[i].Velocity + Math2.Square(DeltaTime) / (2 * mass) * force,
                Velocity = currentState[i].Velocity + (DeltaTime / mass) * force,
            };
        }

        AddOlderState(prevDummyState); // Add it last, as the previous state.
    }

    protected override void StepImpl(ParticleState[] nextState)
    {
        ParticleState[] prevState = PreviousState; // State at time = t - dt
        ParticleState[] currentState = CurrentState; // State at time = t
        // nextState: State at time = (t + dt) (what we're calculating)

        // Calculate all new positions and predicted velocities
        for (int i = 0; i < nextState.Length; i++)
        {
            if (Rails[i] != null)
            {
                predictedStates[i] = new ParticleState
                {
                    Position = Rails[i]!.getPosition(SecondsElapsed),
                    Velocity = Rails[i]!.getVelocity(SecondsElapsed)
                };
                continue;
            }

            float mass = Consts[i].Mass;

            Vector2 force_t = ForceFunction.Apply(currentState, i);
            Vector2 force_t_prev = ForceFunction.Apply(prevState, i);

            Vector2 predictedPosition = currentState[i].Position
                                        + DeltaTime * currentState[i].Velocity
                                        + (2f / 3f * force_t - 1f / 6f * force_t_prev) / mass * Math2.Square(DeltaTime);

            Vector2 predictedVelocity = currentState[i].Velocity + (3f / 2f * force_t - 1f / 2f * force_t_prev) / mass * DeltaTime;

            predictedStates[i] = new ParticleState { Position = predictedPosition, Velocity = predictedVelocity };
        }

        // Now calculate the actual new positions and velocities
        for (int i = 0; i < nextState.Length; i++)
        {
            if (Rails[i] != null)
            {
                nextState[i] = predictedStates[i];
                continue;
            }

            float mass = Consts[i].Mass;

            Vector2 force_t = ForceFunction.Apply(currentState, i);
            Vector2 force_t_prev = ForceFunction.Apply(prevState, i);
            Vector2 force_t_next = ForceFunction.Apply(predictedStates, i);

            nextState[i].Position = predictedStates[i].Position;
            nextState[i].Velocity = currentState[i].Velocity + (1f / 3f * force_t_next + 5f / 6f * force_t - 1f / 6f * force_t_prev) / mass * DeltaTime;
        }
    }
}