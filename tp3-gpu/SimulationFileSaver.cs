using System.Numerics;
using TrippyGL;

namespace tp3_gpu;

public class SimulationFileSaver : IDisposable
{
    public String Filename { get; }

    private readonly BinaryWriter stream;

    private PositionAndVelocity[] tmpbuf;

    public SimulationFileSaver(float containerRadius, ReadOnlySpan<ParticleConsts> particleConsts, String filename)
    {
        Filename = filename;
        stream = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read));
        tmpbuf = new PositionAndVelocity[particleConsts.Length];
        WriteStart(containerRadius, particleConsts);
    }

    private void WriteStart(float containerRadius, ReadOnlySpan<ParticleConsts> particleConsts)
    {
        stream.Write(containerRadius);
        stream.Write((uint)particleConsts.Length);
        // Console.Write($"ContainerRadius={containerRadius}, Particles={particleConsts.Length}");

        for (int i = 0; i < particleConsts.Length; ++i)
        {
            stream.Write(particleConsts[i].Mass);
            stream.Write(particleConsts[i].Radius);
            // Console.Write($", [mass={particleConsts[i].Mass} radius={particleConsts[i].Radius}]");
        }

        // Console.WriteLine(";");
    }

    public void Save(uint step, float time, in ParticleVarsBuffer particleVarsBuffer)
    {
        int particleCount = (int)(particleVarsBuffer.PositionAndVelocity.Width * particleVarsBuffer.PositionAndVelocity.Height);
        if (tmpbuf.Length != particleCount) throw new Exception("File saver was given a state buffer with different size than the initial buffer");

        stream.Write(step);
        stream.Write(time);
        // Console.Write($"Step={step}, time={time}");

        particleVarsBuffer.PositionAndVelocity.Texture.GetData<PositionAndVelocity>(tmpbuf);
        for (int i = 0; i < tmpbuf.Length; ++i)
        {
            stream.Write(tmpbuf[i].Position.X);
            stream.Write(tmpbuf[i].Position.Y);
            stream.Write(tmpbuf[i].Velocity.X);
            stream.Write(tmpbuf[i].Velocity.Y);
            // Console.Write($", [Position={tmpbuf[i].Position} Velocity={tmpbuf[i].Velocity}]");
        }

        // Console.WriteLine(";");
    }

    public void Dispose()
    {
        stream.Flush();
        stream.Dispose();
    }
}
