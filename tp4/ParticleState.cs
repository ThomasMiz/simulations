using Silk.NET.Maths;

namespace tp4;

public struct ParticleState
{
    public Vector2D<double> Position { get; set; }
    public Vector2D<double> Velocity { get; set; }

    public override string ToString()
    {
        return $"Position=({Position.X}, {Position.Y}), Velocity=({Velocity.X}, {Velocity.Y})";
    }
}