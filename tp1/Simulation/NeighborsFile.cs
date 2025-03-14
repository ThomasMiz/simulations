using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace tp1.Simulation;

public static class NeighborsFile
{
    public static void WriteToFile(string file, IEnumerable<(Particle, ICollection<Particle>)> neighbors)
    {
        using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write);
        WriteToStream(new StreamWriter(fileStream), neighbors);
    }

    public static void WriteToStream(Stream stream, IEnumerable<(Particle, ICollection<Particle>)> neighbors)
    {
        WriteToStream(new StreamWriter(stream), neighbors);
    }

    public static void WriteToStream(StreamWriter streamWriter, IEnumerable<(Particle, ICollection<Particle>)> neighbors)
    {
        foreach (var (particle, neighborSet) in neighbors)
        {
            streamWriter.Write(particle.Id);
            foreach (Particle neighbor in neighborSet)
            {
                streamWriter.Write(',');
                streamWriter.Write(neighbor.Id);
            }

            streamWriter.WriteLine();
        }
    }
}