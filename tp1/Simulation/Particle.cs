using System.Numerics;

namespace tp1.Simulation
{
    public class Particle
    {
        public int Id { get; }
        public Vector2 Position { get; set; }
        public float Radius { get; set; }

        public Particle(int id)
        {
            Id = id;
        }

        public string ToString()
        {
            return string.Format("Particle(Id={0}, Position={1}, Radius={2})", Id, Position, Radius);
        }
    }
}
