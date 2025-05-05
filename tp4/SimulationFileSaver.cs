namespace tp4;

public class SimulationFileSaver : IDisposable
{
    private readonly StreamWriter stream;

    public SimulationFileSaver(string filename, uint? saveEveryStep, string integrationType, double deltaTime, ParticleConsts[] particleConsts)
    {
        Filename = filename;
        SaveEverySteps = saveEveryStep ?? 1;
        stream = File.CreateText(filename);
        WriteStart(integrationType, deltaTime, particleConsts);
    }

    public string Filename { get; }
    public uint SaveEverySteps { get; }

    public void Dispose()
    {
        Console.WriteLine($"Saved to {Filename}");
        stream.Flush();
        stream.Dispose();
    }

    private void WriteStart(string integrationType, double deltaTime, ParticleConsts[] particleConsts)
    {
        stream.Write("IntegrationType: ");
        stream.Write(integrationType);
        stream.Write('\n');

        stream.Write("DeltaTime: ");
        stream.Write(deltaTime);
        stream.Write('\n');

        stream.Write("Masses: [");
        for (int i = 0; i < particleConsts.Length; i++)
        {
            if (i != 0) stream.Write(", ");
            stream.Write(particleConsts[i].Mass);
        }

        stream.Write("]\n");
    }

    public void AppendState(uint step, double time, ParticleState[] state)
    {
        if (step % SaveEverySteps != 0)
            return;

        stream.Write('[');
        stream.Write(step);
        stream.Write(' ');
        stream.Write(time);
        stream.Write(']');
        for (int i = 0; i < state.Length; i++)
        {
            stream.Write(i == 0 ? " " : " ; ");
            stream.Write(state[i].Position.X);
            stream.Write(' ');
            stream.Write(state[i].Position.Y);
            stream.Write(' ');
            stream.Write(state[i].Velocity.X);
            stream.Write(' ');
            stream.Write(state[i].Velocity.Y);
        }

        stream.Write('\n');
    }
}