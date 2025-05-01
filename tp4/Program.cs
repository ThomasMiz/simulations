using System.Globalization;

namespace tp4;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        const float A = 1f;
        const float Mass = 70f;
        const float K = 10000f;
        const float Gamma = 100;

        var config = new SimulationConfig()
        {
            DeltaTime = 0.01f,
            MaxSimulationTime = 5,
            OutputFile = "output-{count}-{type}-{steps}steps.txt",
            ForceFunction = ForceFunctions.OsciladorAmortiguado(k: 10000, y: 100)
        }.AddParticle(Mass, position: (1, 0), velocity: (-A * Gamma / (2 * Mass), 0));

        using Simulation verlet = config.BuildVerlet();
        verlet.RunToEnd();

        using Simulation beeman = config.BuildBeeman();
        beeman.RunToEnd();
    }
}