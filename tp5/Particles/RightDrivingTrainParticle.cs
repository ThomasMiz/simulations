using Silk.NET.Maths;

namespace tp5.Particles;

/// <summary>
/// An extension of the Train Promoting SFM particle that adds another customization to the Social Force Model particle
/// that promotes a particle "drives on the right", like cars do.
///
/// The expectation is that in a horizontal hallway, particles going left would tend towards the top and particles going
/// right would tend towards the bottom. 
/// </summary>
public class RightDrivingTrainParticle : TargetHorizontalVelocityParticle
{
    public override string Name => IsLeftToRight ? "TP-Left" : "TP-Right";

    private List<Particle> neighborsList = new();
    public double A { get; set; } = 10;
    public double B { get; set; } = 0.2; //0.08;

    public double RighteousForce { get; set; } = 2;

    private double initialForceEndTime;

    protected override void OnInitializedImpl()
    {
        initialForceEndTime = Simulation.SecondsElapsed + 1;
    }

    public override Vector2D<double> CalculateForce()
    {
        // During the first second/s of existence, focus on moving towards its right
        if (Simulation.SecondsElapsed < initialForceEndTime)
        {
            // Right-driving formula
            double midY = (Simulation.Bounds.Top + Simulation.Bounds.Bottom) / 2;
            if (IsLeftToRight)
            {
                if (Position.Y > midY && Velocity.Y > -TargetHorizontalVelocity)
                    return new Vector2D<double>(0, -Acceleration * 1.5);
            }
            else
            {
                if (Position.Y < midY && Velocity.Y < TargetHorizontalVelocity)
                    return new Vector2D<double>(0, Acceleration * 1.5);
            }
        }

        Vector2D<double> force = Vector2D<double>.Zero;

        Simulation.FindParticlesWithinDistance(this, B, neighborsList);
        foreach (Particle other in neighborsList)
        {
            Vector2D<double> diff = other.Position - Position;

            double distance = diff.Length;
            double e = distance - Radius - other.Radius;

            Vector2D<double> n = diff / distance;
            // Vector2D<double> t = n.Y > 0 ? new(n.Y, -n.X) : new(-n.Y, n.X);

            // Social force
            Vector2D<double> evasiveForce = -A * Math.Exp(-e / B) * n;

            // Train-promoting formula
            if (Velocity != Vector2D<double>.Zero && other.Velocity != Vector2D<double>.Zero)
            {
                double dot = -Vector2D.Dot(Vector2D.Normalize(Velocity), Vector2D.Normalize(other.Velocity));
                evasiveForce *= new Vector2D<double>(Math.Max(dot, 0), dot);
            }

            force += evasiveForce;
        }

        neighborsList.Clear();
        
        {
            // Right-driving formula
            double midY = (Simulation.Bounds.Top + Simulation.Bounds.Bottom) / 2;
            if (IsLeftToRight)
            {
                if (Position.Y > midY)
                    force.Y -= RighteousForce;
            }
            else
            {
                if (Position.Y < midY)
                    force.Y += RighteousForce;
            }
        }

        return force + base.CalculateForce();
    }
}