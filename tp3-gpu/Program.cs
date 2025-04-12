using System.Globalization;
using System.Numerics;

namespace tp3_gpu;

class Program
{
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        SimulationConfig config = new SimulationConfig()
            {
                ContainerRadius = 0.05f,
                OutputFile = "output.sim",
                //MaxSteps = 2000,
                //MaxSimulationTime = 60,
            }
            .AddSingleParticle(mass: 3.0f, radius: 0.005f, position: Vector2.Zero, velocity: new Vector2(0.0000000000000001f))
            .AddScatteredParticles(count: 200, mass: 1.0f, radius: 0.0005f, speed: 1.0f);

        new AnimatorSimulationWindow(config, animationSpeed: 0.05f).Run();
        // new HeadlessSimulationWindow(config).Run();
    }
}