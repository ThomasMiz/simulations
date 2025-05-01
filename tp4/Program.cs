using System.Globalization;

namespace tp4;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Console.WriteLine("Hello, World!");

        var config = new SimulationConfig()
        {
            DeltaTime = 0.01f,
            MaxSimulationTime = 5,
            OutputFile = "output.txt",
            ForceFunction = ForceFunctions.OsciladorAmortiguado(k: 10000, y: 100)
        }.AddParticle(mass: 70, position: (1, 0), velocity: (-10f * 100f / (2f * 70f), 0));

        using Simulation simulation = config.BuildVerlet();
        simulation.RunToEnd();
    }
}