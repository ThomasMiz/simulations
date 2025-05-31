using tp5.ParticleHandlers;
using tp5.Particles;

namespace tp5;

public class SimulationFileSaver : IDisposable
{
    public float? SavingDeltaTime { get; set; }

    public string Filename { get; set; }

    private readonly StreamWriter stream;
    private double nextSaveTime = 0;

    public SimulationFileSaver(string filename)
    {
        stream = File.CreateText(filename);
        Filename = filename;
    }

    public void OnStep(Simulation simulation, float deltaTime, double elapsed)
    {
        if (elapsed >= nextSaveTime)
        {
            while (nextSaveTime < elapsed)
            {
                nextSaveTime += deltaTime;
            }
            
            WriteState(simulation, elapsed);
        }
    }

    private void WriteState(Simulation simulation, double elapsed)
    {
        stream.Write("Time=");
        stream.Write(elapsed);

        foreach (ParticleHandler ph in simulation.ParticleHandlers)
        {
            stream.Write(';');

            stream.Write('[');
            bool isFirstParticle = true;
            foreach (Particle particle in ph.Particles)
            {
                if (isFirstParticle)
                {
                    isFirstParticle = false;
                }
                else
                {
                    stream.Write(',');
                }

                stream.Write("{x=");
                stream.Write(particle.Body.Position.X);
                stream.Write(",y=");
                stream.Write(particle.Body.Position.Y);
                stream.Write(",vx=");
                stream.Write(particle.Body.LinearVelocity.X);
                stream.Write(",vy}");
                stream.Write(particle.Body.LinearVelocity.Y);
            }

            stream.Write(']');
        }
        
        stream.WriteLine();
    }

    public void Dispose()
    {
        Console.WriteLine($"Saved to {Filename}");
        stream.Flush();
        stream.Dispose();
    }
}