using Silk.NET.Maths;

namespace tp5.Particles;

public abstract class Particle
{
    public abstract String Name { get; }
    public abstract bool IsForceVelocityDependant { get; }
    
    public long Id { get; set; }
    public Simulation Simulation { get; set; }

    public double Mass { get; init; }
    public double Radius { get; set; }

    public LinkedListNode<Particle> Node { get; set; }

    public Vector2D<double> Position { get; set; }
    public Vector2D<double> NextPosition { get; set; }
    public Vector2D<double> Velocity { get; set; }
    public Vector2D<double> NextVelocity { get; set; }

    // Auxiliary vectors for exclusive use by the simulation integrator
    public Vector2D<double> Aux0;
    public Vector2D<double> Aux1;
    public Vector2D<double> Aux2;
    public Vector2D<double> Aux3;
    public Vector2D<double> Aux4;
    public Vector2D<double> Aux5;

    public void Remove()
    {
        Simulation.RemoveParticle(this);
    }

    public abstract void OnInitialized();
    public abstract void OnRemoved();

    public abstract Vector2D<double> CalculateForce();

    public abstract Vector2D<double> CalculateDerivative(int derivative);
}