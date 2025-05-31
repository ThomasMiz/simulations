using Silk.NET.Maths;
using tp5.Integration;
using tp5.Particles;

namespace tp5;

public class SimulationConfig
{
    private LinkedList<Particle> particles = new();
    public double DeltaTime { get; set; }
    public double? SavingDeltaTime { get; set; }

    public uint? MaxSteps { get; set; } = null;
    public double? MaxSimulationTime { get; set; } = null;

    public string? OutputFile { get; set; } = null;

    public ForceFunction ForceFunction { get; set; }

    public IntegrationMethod IntegrationMethod { get; set; }

    public SimulationConfig AddParticle(double mass, Vector2D<double> position, Vector2D<double> velocity)
    {
        Particle particle = new ControlledParticle { Mass = mass, Position = position, Velocity = velocity, ForceFunction = this.ForceFunction };
        particle.Node = particles.AddLast(particle);
        return this;
    }

    public SimulationConfig AddParticle(double mass, (double, double) position, (double, double) velocity)
    {
        return AddParticle(mass, new Vector2D<double>(position.Item1, position.Item2), new Vector2D<double>(velocity.Item1, velocity.Item2));
    }

    private void CheckValidity()
    {
        if (ForceFunction == null) throw new ArgumentNullException(nameof(ForceFunction));
        if (particles.Count == 0) throw new ArgumentOutOfRangeException(nameof(particles), particles.Count, "Must specify at least one particle");

        if (MaxSteps == null && MaxSimulationTime == null)
            Console.WriteLine("Warning: simulation has no end condition");
    }

    private uint? CalculateMaxSteps()
    {
        uint? maxStepsByTime = MaxSimulationTime == null ? null : (uint)Math.Ceiling(MaxSimulationTime.Value / DeltaTime - 0.2);

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

        SimulationFileSaver saver = new SimulationFileSaver(MakeOutputFilename(), SavingDeltaTime);
        return new Simulation(IntegrationMethod, DeltaTime, CalculateMaxSteps(), list, saver);
    }
}