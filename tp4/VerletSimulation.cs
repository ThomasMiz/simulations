using System.Numerics;

namespace tp4;

public class VerletSimulation : Simulation
{
    // Verlet requires the positions of the next step to calculate the velocity of the current step, so to make the
    // output always be position+velocity together, the position calculations are always one step ahead.
    private Vector2[] nextStatePositions;

    public VerletSimulation(SimulationConfig config) : base("Verlet", 2, config)
    {
        nextStatePositions = new Vector2[CurrentState.Length];
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
            Vector2 force = ForceFunction(currentState, currentState, i);
            prevDummyState[i] = new ParticleState
            {
                Position = currentState[i].Position - DeltaTime * currentState[i].Velocity + Math2.Square(DeltaTime) / (2 * mass) * force,
                Velocity = currentState[i].Velocity + (DeltaTime / mass) * force,
            };

            // Calculate the positions for the next state
            nextStatePositions[i] = 2 * currentState[i].Position - prevDummyState[i].Position + Math2.Square(DeltaTime) / mass * force;
        }

        States.Add(prevDummyState); // Add it last, as the previous state.
    }

    protected override void StepImpl()
    {
        ParticleState[] currentState = States[0]; // State at time = t

        ParticleState[] nextState = States[^1]; // State at time = (t + dt) (what we're calculating)
        States.RemoveAt(States.Count - 1);

        // Simulation code

        for (int i = 0; i < nextState.Length; i++)
        {
            nextState[i].Position = nextStatePositions[i];
        }

        for (int i = 0; i < nextState.Length; i++)
        {
            // Vector2 force = ForceFunction(currentState, prevState, i); // Force F(t)
            // nextState[i].Position = 2 * currentState[i].Position - prevState[i].Position + Math2.Square(DeltaTime) / mass * force;
            // currentState[i].Velocity = (nextState[i].Position - prevState[i].Position) / (2 * DeltaTime);


            // Calculate r(t + 2dt), which in the next iteration will be r(t + dt)
            Vector2 force = ForceFunction(nextState, currentState, i); // Force F(t + dt)
            nextStatePositions[i] = 2 * nextStatePositions[i] - currentState[i].Position + Math2.Square(DeltaTime) / Consts[i].Mass * force;

            // For cases such as the "Oscilador Amortiguado", where the force F(t) depends on the velocity v(t), the
            // Verlet equations are recursive: to calculate F(t) we need v(t), which needs r(t + dt), which needs F(t).
            // To solve this, we use the most recently available positions and velocities: F(t) = f(r(t), v(t-1))
            nextState[i].Velocity = (nextStatePositions[i] - currentState[i].Position) / (2 * DeltaTime);
        }

        States.Insert(0, nextState);
    }
}