using Silk.NET.Maths;

namespace tp5.Particles;

/// <summary>
/// Customization of Social Force Model particle with an additional train-promoting formula
/// </summary>
public class TrainPromotingSfmParticle : TargetHorizontalVelocityParticle
{
    public override string Name => IsLeftToRight ? "TP-Left" : "TP-Right";

    private List<Particle> neighborsList = new();

    public double Kn { get; set; } = 1.2e5;
    public double A { get; set; } = 10;
    public double B { get; set; } = 0.2; //0.08;

    protected override Vector2D<double> CalculateForceImpl()
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

            // Contact force
            // double vt = Vector2D.Dot(Velocity, t);
            if (e < 0)
            {
                force -= (-e * Kn) * n; // + (e * Kt * vt) * t;
            }
            else
            {
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
        }

        neighborsList.Clear();

        return force + base.CalculateForceImpl();
    }
}