using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Silk.NET.Maths;
using TrippyGL.Utils;

namespace tp1.Simulate
{
    public class Simulation
    {
        public Vector2 Size { get; }
        public Neighbors Neighbors { get; private set; }
        public Dictionary<Particle, HashSet<Particle>> NeighborsDictionary { get; private set; }
        public float NeighborRadius { get; private set; }
        public Vector2D<int> BinCount { get; private set; }

        private List<Particle> particles;
        public IReadOnlyList<Particle> Particles => particles;


        public Simulation(SimulationConfig config)
        {
            particles = config.Particles;
            Size = config.Size;
            NeighborRadius = 1;
            BinCount = new Vector2D<int>(config.M);
        }

        public void Initialize()
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.Write("Calculating neighbors...");
            stopwatch.Start();

            Neighbors = new Neighbors(Size, BinCount, true, particles);
            NeighborsDictionary = Neighbors.FindAllNeighbors(NeighborRadius);
            stopwatch.Stop();
            Console.WriteLine(" Done! Took {0}", stopwatch.Elapsed);

            Console.Write("Writing neighbors file...");
            NeighborsFile.WriteToFile("neighbors.txt", particles.Select(p => (p, (ICollection<Particle>)NeighborsDictionary[p])));
            Console.WriteLine(" Done!");
        }

        public void Step()
        {
            Random random = new Random(123);
            foreach (Particle particle in particles)
            {
                particle.Position += new Vector2(random.NextFloat(), random.NextFloat()) * 0.1f - new Vector2(0.05f);
                particle.Position = new Vector2((particle.Position.X + Size.X) % Size.X, (particle.Position.Y + Size.Y) % Size.Y);
            }

            Neighbors.Recalculate();
            NeighborsDictionary = Neighbors.FindAllNeighbors(NeighborRadius);
        }
    }
}