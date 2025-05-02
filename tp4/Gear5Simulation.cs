using System.Numerics;

namespace tp4;

public class Gear5Simulation : Simulation
{
    public const string TypeName = "gear5";

    // Verlet requires the positions of the next step to calculate the velocity of the current step, so to make the
    // output always be position+velocity together, the position calculations are always one step ahead.
    private PredictedVars[] predictedVars;

    private readonly GearConstants gearConstants;

    public Gear5Simulation(string? outputFile, SimulationConfig config) : base(TypeName, 1, outputFile, config)
    {
        predictedVars = new PredictedVars[CurrentState.Length];
        gearConstants = ForceFunction.IsVelocityDependant ? GearConstants.PositionAndVelocityDependentForces : GearConstants.PositionDependentForces;
    }

    protected override void InitializeImpl()
    {
        for (int i = 0; i < predictedVars.Length; i++)
        {
            predictedVars[i].r0 = ForceFunction.GetDerivative(0, CurrentState, Consts, i);
            predictedVars[i].r1 = ForceFunction.GetDerivative(1, CurrentState, Consts, i);
            predictedVars[i].r2 = ForceFunction.GetDerivative(2, CurrentState, Consts, i);
            predictedVars[i].r3 = ForceFunction.GetDerivative(3, CurrentState, Consts, i);
            predictedVars[i].r4 = ForceFunction.GetDerivative(4, CurrentState, Consts, i);
            predictedVars[i].r5 = ForceFunction.GetDerivative(5, CurrentState, Consts, i);
        }
    }

    protected override void StepImpl(ParticleState[] nextState)
    {
        // nextState: State at time = (t + dt) (what we're calculating)

        // Calculate all the predicted variables
        for (int i = 0; i < nextState.Length; i++)
        {
            PredictedVars prevPreds = predictedVars[i];

            predictedVars[i].r0 = prevPreds.r0 + prevPreds.r1 * DeltaTime + prevPreds.r2 * Math2.Square(DeltaTime) / 2f
                                  + prevPreds.r3 * Math2.Cube(DeltaTime) / 6f + prevPreds.r4 * Math2.Pow4(DeltaTime) / 24f
                                  + prevPreds.r5 * Math2.Pow5(DeltaTime) / 120f;

            predictedVars[i].r1 = prevPreds.r1 + prevPreds.r2 * DeltaTime + prevPreds.r3 * Math2.Square(DeltaTime) / 2f
                                  + prevPreds.r4 * Math2.Cube(DeltaTime) / 6f + prevPreds.r5 * Math2.Pow4(DeltaTime) / 24f;

            predictedVars[i].r2 = prevPreds.r2 + prevPreds.r3 * DeltaTime + prevPreds.r4 * Math2.Square(DeltaTime) / 2f
                                  + prevPreds.r5 * Math2.Cube(DeltaTime) / 6f;

            predictedVars[i].r3 = prevPreds.r3 + prevPreds.r4 * DeltaTime + prevPreds.r5 * Math2.Square(DeltaTime) / 2f;

            predictedVars[i].r4 = prevPreds.r4 + prevPreds.r5 * DeltaTime;

            predictedVars[i].r5 = prevPreds.r5;

            // Used for the force calculations, will be overwritten in the next loop
            nextState[i] = new ParticleState { Position = predictedVars[i].r0, Velocity = predictedVars[i].r1 };
        }

        for (int i = 0; i < nextState.Length; i++)
        {
            Vector2 force = ForceFunction.Apply(nextState, i);
            Vector2 a = force / Consts[i].Mass;

            Vector2 deltaA = a - predictedVars[i].r2;
            Vector2 deltaR2 = deltaA * Math2.Square(DeltaTime) / 2f;

            // Apply corrections
            predictedVars[i].r0 += gearConstants.a0 * deltaR2;
            predictedVars[i].r1 += gearConstants.a1 * deltaR2 / DeltaTime;
            predictedVars[i].r2 += gearConstants.a2 * deltaR2 * 2f / Math2.Square(DeltaTime);
            predictedVars[i].r3 += gearConstants.a3 * deltaR2 * 6f / Math2.Cube(DeltaTime);
            predictedVars[i].r4 += gearConstants.a4 * deltaR2 * 24f / Math2.Pow4(DeltaTime);
            predictedVars[i].r5 += gearConstants.a5 * deltaR2 * 120f / Math2.Pow5(DeltaTime);

            nextState[i] = new ParticleState { Position = predictedVars[i].r0, Velocity = predictedVars[i].r1 };
        }
    }

    struct PredictedVars
    {
        public Vector2 r0, r1, r2, r3, r4, r5;

        public override string ToString()
        {
            return $"r0={r0}, r1={r1}, r2={r2}, r3={r3}, r4={r4}, r5={r5}";
        }
    }
}