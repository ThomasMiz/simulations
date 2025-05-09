using Silk.NET.Maths;

namespace tp4;

public class BeemanSimulation : Simulation
{
    public const string TypeName = "beeman";

    // Verlet requires the positions of the next step to calculate the velocity of the current step, so to make the
    // output always be position+velocity together, the position calculations are always one step ahead.
    private readonly ParticleState[] predictedStates;

    public BeemanSimulation(string? outputFile, SimulationConfig config) : base(TypeName, 3, outputFile, config)
    {
        predictedStates = new ParticleState[CurrentState.Length];
    }

    protected override void InitializeImpl()
    {
        ParticleState[] currentState = CurrentState;
        ParticleState[] prevDummyState = new ParticleState[currentState.Length];

        Parallel.For(0, currentState.Length, i =>
        {
            if (Rails[i] != null)
            {
                prevDummyState[i] = new ParticleState
                {
                    Position = Rails[i]!.getPosition(-DeltaTime),
                    Velocity = Rails[i]!.getVelocity(-DeltaTime)
                };
                return;
            }

            double m = Consts[i].Mass;
            Vector2D<double> a = ForceFunction.Apply(Consts, currentState, i) / m;

            prevDummyState[i] = new ParticleState
            {
                Position = currentState[i].Position - DeltaTime * currentState[i].Velocity + 0.5 * a * Math2.Square(DeltaTime),
                Velocity = currentState[i].Velocity - a * DeltaTime
            };
        });

        AddOlderState(prevDummyState); // Add it last, as the previous state.
    }

    protected override void StepImpl(ParticleState[] nextState)
    {
        ParticleState[] prevState = PreviousState; // State at time = t - dt
        ParticleState[] currentState = CurrentState; // State at time = t
        // nextState: State at time = (t + dt) (what we're calculating)

        Parallel.For(0, nextState.Length, i =>
        {
            if (Rails[i] != null)
            {
                predictedStates[i] = new ParticleState
                {
                    Position = Rails[i]!.getPosition(SecondsElapsed),
                    Velocity = Rails[i]!.getVelocity(SecondsElapsed)
                };
                return;
            }

            double m = Consts[i].Mass;

            Vector2D<double> a_t = ForceFunction.Apply(Consts, currentState, i) / m;
            Vector2D<double> a_tm1 = ForceFunction.Apply(Consts, prevState, i) / m;

            Vector2D<double> x_next = currentState[i].Position
                                      + currentState[i].Velocity * DeltaTime
                                      + (2.0 / 3.0 * a_t - 1.0 / 6.0 * a_tm1) * Math2.Square(DeltaTime);

            Vector2D<double> v_predict = currentState[i].Velocity
                                         + (3.0 / 2.0 * a_t - 1.0 / 2.0 * a_tm1) * DeltaTime;

            predictedStates[i] = new ParticleState
            {
                Position = x_next,
                Velocity = v_predict // provisional
            };
        });

        // Now calculate the actual new positions and velocities
        Parallel.For(0, nextState.Length, i =>
        {
            if (Rails[i] != null)
            {
                nextState[i] = predictedStates[i];
                return;
            }

            double m = Consts[i].Mass;

            Vector2D<double> a_t = ForceFunction.Apply(Consts, currentState, i) / m;
            Vector2D<double> a_tm1 = ForceFunction.Apply(Consts, prevState, i) / m;
            Vector2D<double> a_tp1 = ForceFunction.Apply(Consts, predictedStates, i) / m;

            nextState[i] = new ParticleState
            {
                Position = predictedStates[i].Position,
                Velocity = currentState[i].Velocity
                           + (1.0 / 3.0 * a_tp1 + 5.0 / 6.0 * a_t - 1.0 / 6.0 * a_tm1) * DeltaTime
            };
        });
    }
}