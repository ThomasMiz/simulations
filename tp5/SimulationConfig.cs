using Silk.NET.Maths;
using tp5.Integration;
using tp5.Particles;

namespace tp5;

public class SimulationConfig
{
    private LinkedList<Particle> particles = new();
    public double? DeltaTime { get; set; } = null;
    public double? SavingDeltaTime { get; set; } = null;

    public uint? MaxSteps { get; set; } = null;
    public double? MaxSimulationTime { get; set; } = null;

    public string? OutputFile { get; set; } = null;

    public ForceFunction ForceFunction { get; set; } // TODO: Remove

    public IntegrationMethod? IntegrationMethod { get; set; } = null;

    public SimulationConfig AddParticle(double mass, Vector2D<double> position, Vector2D<double> velocity)
    {
        Particle particle = new ControlledParticle { Mass = mass, Position = position, Velocity = velocity, ForceFunction = this.ForceFunction, Radius = 0.25f};
        particle.Node = particles.AddLast(particle);
        return this;
    }

    public SimulationConfig AddParticle(double mass, (double, double) position, (double, double) velocity)
    {
        return AddParticle(mass, new Vector2D<double>(position.Item1, position.Item2), new Vector2D<double>(velocity.Item1, velocity.Item2));
    }

    private void CheckValidity()
    {
        if (DeltaTime == null) throw new ArgumentNullException(nameof(DeltaTime));
        if (IntegrationMethod == null) throw new ArgumentNullException(nameof(IntegrationMethod));

        if (MaxSteps == null && MaxSimulationTime == null)
            throw new ArgumentException("Simulation has no end condition! Specify either " + nameof(MaxSteps) + " or " + nameof(MaxSimulationTime) + ".");

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

        LinkedList<Particle> list = particles;
        particles = new LinkedList<Particle>(); // Not reusable

        SimulationFileSaver? saver = OutputFile == null ? null : new(MakeOutputFilename(), SavingDeltaTime);

        return new Simulation(IntegrationMethod, DeltaTime.Value, CalculateMaxSteps(), list, saver);
    }
}