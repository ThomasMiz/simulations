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
        Initialize();
    }

    private void Initialize()
    {
        ParticleState[] currentState = States[0];
        ParticleState[] prevDummyState = new ParticleState[currentState.Length];

        for (int i = 0; i < currentState.Length; i++)
        {
            // Create a "dummy" state previous to the real initial state using simple Euler interpolation
            // This is because the verlet algorithm needs the two previous states to calculate the next
            float mass = Consts[i].Mass;
            Vector2 force = ForceFunction(currentState, i);
            prevDummyState[i] = new ParticleState
            {
                Position = currentState[i].Position - DeltaTime * currentState[i].Velocity + Math2.Square(DeltaTime) / (2 * mass) * force,
                Velocity = currentState[i].Velocity + (DeltaTime / mass) * force,
            };
        }

        States.Add(prevDummyState); // Add it last, as the previous state.
    }

    protected override void StepImpl()
    {
        ParticleState[] prevState = States[1]; // State at time = t - dt
        ParticleState[] currentState = States[0]; // State at time = t

        ParticleState[] nextState = States[^1]; // State at time = (t + dt) (what we're calculating)
        States.RemoveAt(States.Count - 1);

        // Simulation code

        // Calculate all new positions and predicted velocities
        for (int i = 0; i < nextState.Length; i++)
        {
            float mass = Consts[i].Mass;

            Vector2 force_t = ForceFunction(currentState, i);
            Vector2 force_t_prev = ForceFunction(prevState, i);

            Vector2 predictedPosition = currentState[i].Position
                                        + DeltaTime * currentState[i].Velocity
                                        + (2f / 3f * force_t - 1f / 6f * force_t_prev) / mass * Math2.Square(DeltaTime);

            Vector2 predictedVelocity = currentState[i].Velocity + (3f / 2f * force_t - 1f / 2f * force_t_prev) / mass * DeltaTime;

            predictedStates[i] = new ParticleState { Position = predictedPosition, Velocity = predictedVelocity };
        }

        for (int i = 0; i < nextState.Length; i++)
        {
            float mass = Consts[i].Mass;

            Vector2 force_t = ForceFunction(currentState, i);
            Vector2 force_t_prev = ForceFunction(prevState, i);
            Vector2 force_t_next = ForceFunction(predictedStates, i);

            nextState[i].Position = predictedStates[i].Position;
            nextState[i].Velocity = currentState[i].Velocity + (1f / 3f * force_t_next + 5f / 6f * force_t - 1f / 6f * force_t_prev) / mass * DeltaTime;
        }

        States.Insert(0, nextState);
    }
}