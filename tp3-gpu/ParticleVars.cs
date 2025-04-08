using System.Numerics;
using System.Runtime.InteropServices;

namespace tp3_gpu;

[StructLayout(LayoutKind.Sequential)]
public struct ParticleVars
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public ParticleVars(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }
}