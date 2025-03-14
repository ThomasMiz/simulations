using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Stopwatch stopwatch = new Stopwatch();
            Console.Write("Calculating neighbors...");
            stopwatch.Start();
            Neighbors neighbors = new Neighbors(Size, Size / 16, true, particles);
            Dictionary<Particle, HashSet<Particle>> result = neighbors.FindAllNeighbors(10);
            stopwatch.Stop();
            Console.WriteLine(" Done! Took {0}", stopwatch.Elapsed);

            Console.Write("Writing neighbors file...");
            NeighborsFile.WriteToFile("neighbors.txt", particles.Select(p => (p, (ICollection<Particle>)result[p])));
            Console.WriteLine(" Done!");
        }

        public void Step()
        {
        }
    }
}