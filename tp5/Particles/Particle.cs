using Silk.NET.Maths;

namespace tp5.Particles;

public class Particle
{
    public long Id { get; set; }

    public double Mass { get; init; }
    public virtual double Radius { get; set; }

    public LinkedListNode<Particle> Node { get; set; }

    public virtual Vector2D<double> Position { get; set; }
    public virtual Vector2D<double> NextPosition { get; set; }
    public virtual Vector2D<double> Velocity { get; set; }
    public virtual Vector2D<double> NextVelocity { get; set; }

    // Auxiliary vectors for exclusive use by the simulation integrator
    public Vector2D<double> Aux0;
    public Vector2D<double> Aux1;
    public Vector2D<double> Aux2;
    public Vector2D<double> Aux3;
    public Vector2D<double> Aux4;
    public Vector2D<double> Aux5;
}