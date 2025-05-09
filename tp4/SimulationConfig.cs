using Silk.NET.Maths;

namespace tp4;

public class SimulationConfig
{
    private readonly List<ParticleConsts> consts = new();
    private readonly List<ParticleState> initialState = new();
    private readonly List<ParticleRail?> rails = new();
    public double DeltaTime { get; set; }

    public uint? MaxSteps { get; set; } = null;
    public double? MaxSimulationTime { get; set; } = null;

    public string? OutputFile { get; set; } = null;
    public uint SaveEverySteps { get; set; } = 1;

    public ForceFunction ForceFunction { get; set; }

    public ParticleConsts[] GetConstsArray()
    {
        return consts.ToArray();
    }

    public ParticleRail?[] GetRailsArray()
    {
        return rails.ToArray();
    }

    public ParticleState[] GetInitialStateArray()
    {
        return initialState.ToArray();
    }

    public SimulationConfig AddParticle(double mass, Vector2D<double> position, Vector2D<double> velocity)
    {
        consts.Add(new ParticleConsts { Mass = mass });
        rails.Add(null);
        initialState.Add(new ParticleState { Position = position, Velocity = velocity });

        return this;
    }

    public SimulationConfig AddParticle(double mass, (double, double) position, (double, double) velocity)
    {
        return AddParticle(mass, new Vector2D<double>(position.Item1, position.Item2), new Vector2D<double>(velocity.Item1, velocity.Item2));
    }

    public SimulationConfig AddRailedParticle(double mass, ParticleRail rail)
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
        uint? maxStepsByTime = MaxSimulationTime == null ? null : (uint)Math.Ceiling(MaxSimulationTime.Value / DeltaTime - 0.2);

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