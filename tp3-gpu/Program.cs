using System.Globalization;
using System.Numerics;
using SimulationBase;

namespace tp3_gpu;

class Program
{
    private const float BASICALLY_ZERO = 0.0000000000000001f;
    private const float VERY_HIGH = 9999999999.99999999999999999999999999f;

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
            .AddSingleParticle(mass: 3.0f, radius: 0.005f, position: Vector2.Zero, velocity: new Vector2(BASICALLY_ZERO))
            .AddScatteredParticles(count: 250, mass: 1.0f, radius: 0.0005f, speed: 1.0f);

        new AnimatorSimulationWindow(config, animationSpeed: 1).Run();

        // RunSims(fixedObstacle: true, particleCount: 250, speed: 1, 30000, runs: 16);
        // RunSims(fixedObstacle: true, particleCount: 250, speed: 3, 30000, runs: 16);
        // RunSims(fixedObstacle: true, particleCount: 250, speed: 6, 30000, runs: 16);
        // RunSims(fixedObstacle: true, particleCount: 250, speed: 10, 30000, runs: 16);
        // RunSims(fixedObstacle: false, particleCount: 250, speed: 1, 30000, runs: 16);
    }

    private static void RunSims(bool fixedObstacle, int particleCount, float speed, uint maxSteps, int runs, bool visualize = false)
    {
        string obstacleTypeStr = fixedObstacle ? "fixed" : "moving";

        for (int i = 0; i < runs; i++)
        {
            SimulationConfig config = new SimulationConfig()
                {
                    ContainerRadius = 0.05f,
                    OutputFile = $"output{i + 1}-{obstacleTypeStr}obstacle-{particleCount}particles-vel{speed}-{maxSteps / 1000}ksteps.sim",
                    MaxSteps = maxSteps,
                    //MaxSimulationTime = 60,
                }
                .AddSingleParticle(mass: fixedObstacle ? VERY_HIGH : 3.0f, radius: 0.005f, position: Vector2.Zero, velocity: new Vector2(fixedObstacle ? 0 : BASICALLY_ZERO))
                .AddScatteredParticles(count: 250, mass: 1.0f, radius: 0.0005f, speed: speed);

            using WindowBase window = visualize ? new AnimatorSimulationWindow(config, autoClose: i + 1 == runs) : new HeadlessSimulationWindow(config);
            window.Run();
        }
    }
}