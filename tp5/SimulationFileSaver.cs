using tp5.Particles;

namespace tp5;

public class SimulationFileSaver : IDisposable
{
    public double? SavingDeltaTime { get; set; }

    public string Filename { get; set; }

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
    }

    public void OnStep(Simulation simulation)
    {
        double elapsed = simulation.SecondsElapsed;

        if (elapsed >= nextSaveTime)
        {
            do
            {
                nextSaveTime += simulation.DeltaTime;
            } while (elapsed > nextSaveTime);

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

    public void Dispose()
    {
        Console.WriteLine($"Saved to {Filename}");
        stream.Flush();
        stream.Dispose();
    }
}