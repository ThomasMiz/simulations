using Silk.NET.Maths;

namespace tp5.Particles;

public abstract class Particle
{
    private Vector2D<double> _minPosition, _maxPosition;
    private Vector2D<double> _position, _nextPosition;
    private double _radius;

    public abstract String Name { get; }
    public abstract bool IsForceVelocityDependant { get; }

    public long Id { get; set; }
    public Simulation Simulation { get; set; }

    public LinkedListNode<Particle> Node { get; set; }

    public double Mass { get; init; }

    private void CalculateMinMaxPositions()
    {
        if (Simulation == null) return;

        _minPosition = Simulation.Bounds.BottomLeft + new Vector2D<double>(_radius);
        _maxPosition = Simulation.Bounds.TopRight - new Vector2D<double>(_radius);
    }

    private void ClampPositionToBounds()
    {
        if (Simulation == null) return;

        _position = Vector2D.Clamp(_position, _minPosition, _maxPosition);
    }

    public double Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            CalculateMinMaxPositions();
            ClampPositionToBounds();
        }
    }

    public Vector2D<double> Position
    {
        get => _position;
        set
        {
            _position = value;
            ClampPositionToBounds();
        }
    }

    public Vector2D<double> NextPosition
    {
        get => _nextPosition;
        set
        {
            _nextPosition = value;
            ClampPositionToBounds();
        }
    }

    public Vector2D<double> Velocity { get; set; }
    public Vector2D<double> NextVelocity { get; set; }

    public void OnInitialized()
    {
        CalculateMinMaxPositions();
        ClampPositionToBounds();

        OnInitializedImpl();
    }

    public void Remove()
    {
        Simulation.RemoveParticle(this);
    }

    protected abstract void OnInitializedImpl();

    public abstract void PostUpdate();

    public abstract void OnRemoved();

    public abstract Vector2D<double> CalculateForce();

    public abstract Vector2D<double> CalculateDerivative(int derivative);

    // Auxiliary vectors for exclusive use by the simulation integrator
    public Vector2D<double> Aux0;
    public Vector2D<double> Aux1;
    public Vector2D<double> Aux2;
    public Vector2D<double> Aux3;
    public Vector2D<double> Aux4;
    public Vector2D<double> Aux5;
}