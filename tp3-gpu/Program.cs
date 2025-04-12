using System.Globalization;

namespace tp3_gpu;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        new ParticleSimulationWindow().Run();
        // new HeadlessSimRunner().Run();
    }
}