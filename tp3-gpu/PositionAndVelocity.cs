using System.Numerics;
using System.Runtime.InteropServices;

namespace tp3_gpu;

[StructLayout(LayoutKind.Sequential)]
public struct PositionAndVelocity
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public PositionAndVelocity(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public override string ToString()
    {
        return $"Position: {Position}, Velocity: {Velocity}";
    }
}