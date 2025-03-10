using System.Collections.Generic;
using System.Numerics;

namespace tp1.Simulation
{
    public class Simulation
    {
        public Vector2 Size { get; }

        private List<Particle> particles;
        public IReadOnlyList<Particle> Particles => particles;

        public Simulation(SimulationConfig config)
        {
            particles = config.Particles;
            Size = config.Size;
        }

        public void Initialize()
        {

        }

        public void Step()
        {

        }
    }
}
