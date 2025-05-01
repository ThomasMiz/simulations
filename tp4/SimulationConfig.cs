using System.Numerics;

namespace tp4;

public class SimulationConfig
{
    public float DeltaTime { get; init; }

    public uint? MaxSteps { get; init; } = null;
    public float? MaxSimulationTime { get; init; } = null;

    public string? OutputFile { get; init; } = null;

    public ForceFunction ForceFunction { get; init; }

    private List<ParticleConsts> consts = new();
    private List<ParticleState> initialState = new();

    public ParticleConsts[] GetConstsArray() => consts.ToArray();
    public ParticleState[] GetInitialStateArray() => initialState.ToArray();

    public SimulationConfig AddParticle(float mass, Vector2 position, Vector2 velocity)
    {
        consts.Add(new ParticleConsts { Mass = mass });
        initialState.Add(new ParticleState { Position = position, Velocity = velocity });

        return this;
    }

    public SimulationConfig AddParticle(float mass, (float, float) position, (float, float) velocity)
    {
        return AddParticle(mass, new Vector2(position.Item1, position.Item2), new Vector2(velocity.Item1, velocity.Item2));
    }

    private void CheckValidity()
    {
        if (ForceFunction == null) throw new ArgumentNullException(nameof(ForceFunction));
        if (consts.Count == 0) throw new ArgumentOutOfRangeException(nameof(consts), consts.Count, "Must specify at least one particle");

        if (MaxSteps == null && MaxSimulationTime == null)
            Console.WriteLine("Warning: simulation has no end condition");
    }

    public uint? CalculateMaxSteps()
    {
        uint? maxStepsByTime = MaxSimulationTime == null ? null : (uint)Math.Ceiling(MaxSimulationTime.Value / (double)DeltaTime);

        if (maxStepsByTime != null && MaxSteps != null) return Math.Min(maxStepsByTime.Value, MaxSteps.Value);
        return maxStepsByTime ?? MaxSteps;
    }

    private string? MakeOutputFilename(string integrationType)
    {
        if (OutputFile == null) return null;

        return OutputFile
            .Replace("{type}", integrationType)
            .Replace("{steps}", CalculateMaxSteps().ToString())
            .Replace("{count}", consts.Count.ToString());
    }

    public Simulation BuildVerlet()
    {
        CheckValidity();
        return new VerletSimulation(MakeOutputFilename(VerletSimulation.TypeName), this);
    }

    public Simulation BuildBeeman()
    {
        CheckValidity();
        return new BeemanSimulation(MakeOutputFilename(BeemanSimulation.TypeName), this);
    }
}