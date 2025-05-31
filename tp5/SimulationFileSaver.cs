using tp5.Particles;

namespace tp5;

public class SimulationFileSaver : IDisposable
{
    public double? SavingDeltaTime { get; }

    public string Filename { get; }

    private readonly StreamWriter stream;
    private double nextSaveTime = 0;

    public SimulationFileSaver(string filename, double? savingDeltaTime)
    {
        stream = File.CreateText(filename);
        Filename = filename;
        SavingDeltaTime = savingDeltaTime;
    }

    public void WriteStart(Simulation simulation)
    {
        stream.Write("{\"integrationType\": \"");
        stream.Write(simulation.IntegrationMethod.Name);
        stream.Write("\", \"deltaTime\": ");
        stream.Write(simulation.DeltaTime);
        stream.Write("}\n");

        WriteState(simulation);

        if (SavingDeltaTime.HasValue)
        {
            nextSaveTime = SavingDeltaTime.Value;
        }
    }

    public void OnStep(Simulation simulation)
    {
        double elapsed = simulation.SecondsElapsed;

        bool shouldSave = false;

        if (!SavingDeltaTime.HasValue)
        {
            shouldSave = true;
        }
        else if (elapsed >= nextSaveTime)
        {
            shouldSave = true;
            nextSaveTime += SavingDeltaTime.Value;
        }

        if (shouldSave)
        {
            WriteState(simulation);
        }
    }

    public void WriteState(Simulation simulation)
    {
        stream.Write("{\"step\": ");
        stream.Write(simulation.Steps);
        stream.Write(", \"time\": ");
        stream.Write(simulation.SecondsElapsed);
        stream.Write("}");

        foreach (Particle p in simulation.Particles)
        {
            stream.Write(" ; {\"id\": ");
            stream.Write(p.Id);
            stream.Write(", \"name\": \"");
            stream.Write(p.Name);
            stream.Write("\", \"mass\": ");
            stream.Write(p.Mass);
            stream.Write(", \"radius\": ");
            stream.Write(p.Radius);
            stream.Write(", \"x\": ");
            stream.Write(p.Position.X);
            stream.Write(", \"y\": ");
            stream.Write(p.Position.Y);
            stream.Write(", \"vx\": ");
            stream.Write(p.Velocity.X);
            stream.Write(", \"vy\": ");
            stream.Write(p.Velocity.Y);
            stream.Write('}');
        }

        stream.Write('\n');
    }

    public void Dispose()
    {
        if (nextSaveTime == 0)
        {
            Console.WriteLine($"Warning: empty output file. Is the SavingDeltaTime too high? File {Filename}");
        }
        else
        {
            Console.WriteLine($"Saved to {Filename}");
        }

        stream.Flush();
        stream.Dispose();
    }
}