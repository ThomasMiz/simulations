using System.Numerics;
using System.Runtime.InteropServices;

namespace tp3_gpu;

[StructLayout(LayoutKind.Sequential)]
public struct TimeToCollisionAndCollidesWith
{
    public float TimeToCollision { get; set; }
    public Vector2 CollidesWith { get; set; }

    public TimeToCollisionAndCollidesWith(float timeToCollision, Vector2 collidesWith)
    {
        TimeToCollision = timeToCollision;
        CollidesWith = collidesWith;
    }

    public override string ToString()
    {
        return $"TimeToCollision: {TimeToCollision}, CollidesWith: {CollidesWith}";
    }
}