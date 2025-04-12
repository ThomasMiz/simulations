using System.Globalization;
using System.Numerics;
using Silk.NET.Maths;
using SimulationBase;
using TrippyGL.Utils;

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
        simulation.Step();
        Console.WriteLine($"Ran step {simulation.Steps} with next collision time {simulation.SecondsElapsed}");
    }

    protected override void OnResized(Vector2D<int> size)
    {
    }

    protected override void OnUnload()
    {
        simulation.Dispose();
    }
}