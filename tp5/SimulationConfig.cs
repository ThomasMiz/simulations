using Silk.NET.Maths;
using tp5.Integration;
using tp5.Particles;
using tp5.Spawners;

namespace tp5;

public class SimulationConfig
{
    private List<Particle> particles = new();
    private List<ParticleSpawner> particleSpawners = new();

    public double? DeltaTime { get; set; } = null;
    public double? SavingDeltaTime { get; set; } = null;
    public Bounds? SimulationBounds { get; set; } = null;

    public uint? MaxSteps { get; set; } = null;
    public double? MaxSimulationTime { get; set; } = null;

    public string? OutputFile { get; set; } = null;

    public IntegrationMethod? IntegrationMethod { get; set; } = null;

    public SimulationConfig AddParticle(Particle particle)
    {
        particles.Add(particle);
        return this;
    }

    public SimulationConfig AddParticleSpawner(ParticleSpawner spawner)
    {
        particleSpawners.Add(spawner);
        return this;
    }

    private void CheckValidity()
    {
        if (DeltaTime == null) throw new ArgumentNullException(nameof(DeltaTime));
        if (IntegrationMethod == null) throw new ArgumentNullException(nameof(IntegrationMethod));
        if (SimulationBounds == null) throw new ArgumentNullException(nameof(SimulationBounds));

        if (OutputFile == null) Console.WriteLine("Warning: no output file, simulation state will not be saved");
    }

    private uint? CalculateMaxSteps()
    {
        uint? maxStepsByTime = MaxSimulationTime == null || DeltaTime == null ? null : (uint)Math.Ceiling(MaxSimulationTime.Value / DeltaTime.Value - 0.2);

        if (maxStepsByTime != null && MaxSteps != null) return Math.Min(maxStepsByTime.Value, MaxSteps.Value);
        return maxStepsByTime ?? MaxSteps;
    }

    private string? MakeOutputFilename()
    {
        if (OutputFile == null) return null;

        return OutputFile
            .Replace("{type}", IntegrationMethod.Name)
            .Replace("{steps}", CalculateMaxSteps().ToString())
            .Replace("{count}", particles.Count.ToString());
    }

    public Simulation Build()
    {
        CheckValidity();

        // These lists are not reusable
        List<ParticleSpawner> spawnersList = particleSpawners;
        List<Particle> particleList = particles;
        particleSpawners = new List<ParticleSpawner>();
        particles = new List<Particle>();

        SimulationFileSaver? saver = OutputFile == null ? null : new(MakeOutputFilename(), SavingDeltaTime);

        return new Simulation(IntegrationMethod, DeltaTime.Value, CalculateMaxSteps(), SimulationBounds.Value, particleList, spawnersList, saver);
    }
}