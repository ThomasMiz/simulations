using Silk.NET.Maths;
using SimulationBase;

namespace tp3_gpu;

public class HeadlessSimulationWindow : WindowBase
{
    private SimulationConfig config;

    private ParticleSimulation simulation;

    public HeadlessSimulationWindow(SimulationConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));

        Window.FramesPerSecond = 0;
        Window.VSync = false;
    }

    protected override void OnLoad()
    {
        simulation = new ParticleSimulation(graphicsDevice, config);
    }

    protected override void OnRender(double dt)
    {
        if ((config.MaxSteps == null || simulation.Steps < config.MaxSteps) && (config.MaxSimulationTime == null || simulation.SecondsElapsed < config.MaxSimulationTime))
        {
            simulation.Step();

            bool limitReached = false;
            if (config.MaxSimulationTime != null && simulation.SecondsElapsed >= config.MaxSimulationTime)
            {
                Console.WriteLine($"Reached time limit of {simulation.SecondsElapsed} after {simulation.Steps} steps");
                limitReached = true;
            }
            else if (config.MaxSteps != null && simulation.Steps >= config.MaxSteps)
            {
                Console.WriteLine($"Reached step limit of {simulation.Steps} after simulating {simulation.SecondsElapsed} seconds");
                limitReached = true;
            }
            else if (simulation.Steps % 1000 == 0)
            {
                Console.WriteLine($"Ran step {simulation.Steps} with next collision time {simulation.SecondsElapsed}");
            }

            if (limitReached)
            {
                Window.Close();
            }
        }
    }

    protected override void OnResized(Vector2D<int> size)
    {
    }

    protected override void OnUnload()
    {
        simulation.Dispose();
    }
}