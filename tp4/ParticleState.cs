using System.Numerics;

namespace tp4;

public struct ParticleState
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public override string ToString()
    {
        return $"Position=({Position.X}, {Position.Y}), Velocity=({Velocity.X}, {Velocity.Y})";
    }
}