using Silk.NET.Maths;

namespace tp5.Particles;

public class SocialForceParticle : TargetHorizontalVelocityParticle
{
    public override string Name => IsLeftToRight ? "SFM-Left" : "SFM-Right";

    private List<Particle> neighborsList = new();

    public double Kn { get; set; } = 1.2e5;
    public double Kt { get; set; } = 2.4e5;
    public double A { get; set; } = 10;
    public double B { get; set; } = 0.2;//0.08;

    public override Vector2D<double> CalculateForce()
    {
        Vector2D<double> force = Vector2D<double>.Zero;
        
        Simulation.FindParticlesWithinDistance(this, B, neighborsList);
        foreach (Particle other in neighborsList)
        {
            Vector2D<double> diff = other.Position - Position;

            double distance = diff.Length;
            double e = distance - Radius - other.Radius;

            Vector2D<double> n = diff / distance;
            
            // Vector2D<double> t = n.Y > 0 ? new(n.Y, -n.X) : new(-n.Y, n.X);

            // Contact force | Not used as simulation prevents overlaps
            //double vt = Vector2D.Dot(Velocity, t);
            //force += (-e * Kn) * n + (-e * Kt * vt) * t;
            
            // Social force
            force -= A * Math.Exp(-e / B) * n;
        }

        neighborsList.Clear();

        return force + base.CalculateForce();
    }
}