using tp5.Particles;

namespace tp5;

/*public class SimulationFileSaver : IDisposable
{
    private readonly StreamWriter stream;

    public SimulationFileSaver(string filename, uint? saveEveryStep, string integrationType, double deltaTime)
    {
        Filename = filename;
        SaveEverySteps = saveEveryStep ?? 1;
        stream = File.CreateText(filename);
        WriteStart(integrationType, deltaTime);
    }

    public string Filename { get; }
    public uint SaveEverySteps { get; }

    public void Dispose()
    {
        Console.WriteLine($"Saved to {Filename}");
        stream.Flush();
        stream.Dispose();
    }

    private void WriteStart(string integrationType, double deltaTime)
    {
        stream.Write("{\"integrationType\": \"");
        stream.Write(integrationType);
        stream.Write("\", \"deltaTime\": ");
        stream.Write(deltaTime);
        stream.Write("}\n");
    }

    public void AppendState(uint step, double time, IReadOnlyCollection<Particle> particles)
    {
        if (step % SaveEverySteps != 0)
            return;

        stream.Write("{\"step\": ");
        stream.Write(step);
        stream.Write(", \"time\": ");
        stream.Write(time);
        stream.Write("}");

        foreach (Particle p in particles)
        {
            stream.Write(" ; {\"id\": \"");
            stream.Write(p.Id);
            stream.Write("\", \"mass\": \"");
            stream.Write(p.Mass);
            stream.Write("\", \"radius\": \"");
            stream.Write(p.Radius);
            stream.Write("\", \"x\": \"");
            stream.Write(p.Position.X);
            stream.Write("\", \"y\": \"");
            stream.Write(p.Position.Y);
            stream.Write("\", \"vx\": \"");
            stream.Write(p.Velocity.X);
            stream.Write("\", \"vy\": \"");
            stream.Write(p.Velocity.Y);
            stream.Write("\"}");
        }

        stream.Write('\n');
    }
}*/