using System.Globalization;
using tp2;

namespace tp3_gpu;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        new ParticleSimulationWindow().Run();
    }
}