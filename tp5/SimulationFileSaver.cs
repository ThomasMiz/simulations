using tp5.Particles;

namespace tp5;

public class SimulationFileSaver : IDisposable
{
    private readonly StreamWriter stream;

    public SimulationFileSaver(string filename, uint? saveEveryStep, string integrationType, double deltaTime, IReadOnlyCollection<Particle> particles)
    {
        Filename = filename;
        SaveEverySteps = saveEveryStep ?? 1;
        stream = File.CreateText(filename);
        WriteStart(integrationType, deltaTime, particles);
    }

    public string Filename { get; }
    public uint SaveEverySteps { get; }

    public void Dispose()
    {
        Console.WriteLine($"Saved to {Filename}");
        stream.Flush();
        stream.Dispose();
    }

    private void WriteStart(string integrationType, double deltaTime, IReadOnlyCollection<Particle> particles)
    {
        stream.Write("IntegrationType: ");
        stream.Write(integrationType);
        stream.Write('\n');

        stream.Write("DeltaTime: ");
        stream.Write(deltaTime.ToString("G17"));
        stream.Write('\n');

        stream.Write("Masses: [");
        bool isFirst = true;
        foreach (Particle particle in particles)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                stream.Write(", ");
            }

            stream.Write(particle.Mass.ToString("G17"));
        }

        stream.Write("]\n");
    }

    public void AppendState(uint step, double time, IReadOnlyCollection<Particle> particles)
    {
        if (step % SaveEverySteps != 0)
            return;

        stream.Write('[');
        stream.Write(step);
        stream.Write(' ');
        stream.Write(time.ToString("G17"));
        stream.Write(']');

        bool isFirst = true;
        foreach (Particle particle in particles)
        {
            stream.Write(isFirst ? " " : " ; ");
            isFirst = false;
            stream.Write(particle.Position.X.ToString("G17"));
            stream.Write(' ');
            stream.Write(particle.Position.Y.ToString("G17"));
            stream.Write(' ');
            stream.Write(particle.Velocity.X.ToString("G17"));
            stream.Write(' ');
            stream.Write(particle.Velocity.Y.ToString("G17"));
        }

        stream.Write('\n');
    }
}