using System.Numerics;

namespace tp4;

public class SimulationConfig
{
    public float DeltaTime { get; set; }

    public uint? MaxSteps { get; set; } = null;
    public float? MaxSimulationTime { get; set; } = null;

    public string? OutputFile { get; set; } = null;
    public uint SaveEverySteps { get; set; } = 1;

    public ForceFunction ForceFunction { get; set; }

    private List<ParticleConsts> consts = new();
    private List<ParticleRail?> rails = new();
    private List<ParticleState> initialState = new();

    public ParticleConsts[] GetConstsArray() => consts.ToArray();
    public ParticleRail?[] GetRailsArray() => rails.ToArray();
    public ParticleState[] GetInitialStateArray() => initialState.ToArray();

    public SimulationConfig AddParticle(float mass, Vector2 position, Vector2 velocity)
    {
        consts.Add(new ParticleConsts { Mass = mass });
        rails.Add(null);
        initialState.Add(new ParticleState { Position = position, Velocity = velocity });

        return this;
    }

    public SimulationConfig AddParticle(float mass, (float, float) position, (float, float) velocity)
    {
        return AddParticle(mass, new Vector2(position.Item1, position.Item2), new Vector2(velocity.Item1, velocity.Item2));
    }

    public SimulationConfig AddRailedParticle(float mass, ParticleRail rail)
    {
        consts.Add(new ParticleConsts { Mass = mass });
        rails.Add(rail);
        initialState.Add(new ParticleState { Position = rail.getPosition(0), Velocity = rail.getVelocity(0) });
        return this;
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

    public Simulation BuildGear5()
    {
        CheckValidity();
        return new Gear5Simulation(MakeOutputFilename(Gear5Simulation.TypeName), this);
    }
}