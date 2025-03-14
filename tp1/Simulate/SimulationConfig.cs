using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TrippyGL.Utils;
using System.Globalization;

namespace tp1.Simulate
{
    /// <summary>
    /// A formatted text file for describing a simulation's starting point, specifying the simulation's size and particles.
    ///
    /// Also known as the "static file"
    /// </summary>
    public class SimulationConfig
    {
        private static readonly char[] WhitespaceSeparatorChars = [' ', '\t'];

        public Vector2 Size { get; set; }
        public List<Particle> Particles { get; set; }
        public int M { get; set; }
        public static SimulationConfig FromFile(string file)
        {
            using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return FromStream(fs);
        }

        public static SimulationConfig FromStream(Stream stream)
        {
            return FromStream(new StreamReader(stream));
        }

        public static SimulationConfig FromStream(StreamReader streamReader)
        {
            Random random = new();

            string? line;
            int lineNumber = 1;

            try
            {
                // Ignore the particle count heading (we read until the file's end)
                line = streamReader.ReadLine() ?? throw new Exception("The file has no particle count header line");
                lineNumber++;

                // Get the width and height of the simulation. If only width is specified, use that as height.
                line = streamReader.ReadLine() ?? throw new Exception("The file has no simulation size header line");
                string[] split = line.Split(WhitespaceSeparatorChars,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (split.Length == 0) throw new Exception("The file has no simulation size header line");
                float width = float.Parse(split[0], CultureInfo.InvariantCulture);
                float height = split.Length == 1 ? width : float.Parse(split[1], CultureInfo.InvariantCulture);

                lineNumber++;

                List<Particle> particles = new();
                int particleId = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    split = line.Split(WhitespaceSeparatorChars,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    float radius = float.Parse(split[0], CultureInfo.InvariantCulture);
                    float property = split.Length > 1 && split[1] != "-" ? float.Parse(split[1], CultureInfo.InvariantCulture) : 0;
                    float x = split.Length > 2 && split[2] != "-" ? float.Parse(split[2], CultureInfo.InvariantCulture) : random.NextFloat(width);
                    float y = split.Length > 3 && split[3] != "-" ? float.Parse(split[3], CultureInfo.InvariantCulture) : random.NextFloat(height);

                    particles.Add(new Particle(particleId++)
                    {
                        Position = new Vector2(x, y),
                        Radius = radius,
                    });

                    lineNumber++;
                }

                // TODO: Remove, only used to test border condition neighobrs
                //particles[0].Position = new Vector2(0.1f, 0.1f);
                //particles[1].Position = new Vector2(0.2f, 0.2f);
                //particles[2].Position = new Vector2(width - 0.1f, height - 0.1f);

                return new SimulationConfig
                {
                    Size = new Vector2(width, height),
                    Particles = particles,
                };
            }
            catch (Exception e)
            {
                throw new Exception("Error at line " + lineNumber + ": " + e.Message, e);
            }
        }
        
        public static SimulationConfig ReadM(SimulationConfig config, int m)
        {
            config.M = m;
            return config;
        }
    }
}