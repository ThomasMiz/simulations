namespace tp4;

public class SimulationFileSaver : IDisposable
{
    public String Filename { get; }

    private readonly StreamWriter stream;

    public SimulationFileSaver(String filename, ParticleConsts[] particleConsts)
    {
        Filename = filename;
        stream = File.CreateText(filename);
        WriteStart(particleConsts);
    }

    private void WriteStart(ParticleConsts[] particleConsts)
    {
        stream.Write("Masses: [");
        for (int i = 0; i < particleConsts.Length; i++)
            stream.Write(particleConsts[i].Mass);
        stream.Write("]\n");
    }

    public void AppendState(uint step, float time, ParticleState[] state)
    {
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

    public void Dispose()
    {
        stream.Flush();
        stream.Dispose();
    }
}