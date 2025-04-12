using System.Globalization;
using System.Numerics;
using Silk.NET.Maths;
using SimulationBase;
using TrippyGL.Utils;

namespace tp3_gpu;

public class HeadlessSimRunner : WindowBase
{
    private const float SimulationSpeed = 1f;
    const int SimSizeX = 200, SimSizeY = 1;
    const int ParticleCount = SimSizeX * SimSizeY;
    const float ParticleMass = 1;
    const float ParticleRadius = 0.0005f;

    private const float ContainerRadius = 0.05f;
    private const float InnerContainerRadius = 0.005f;

    private const uint MaxSteps = uint.MaxValue;

    private Random r = new Random();

    private ParticleSimulation simulation;

    public HeadlessSimRunner()
    {
        Window.FramesPerSecond = 0;
        Window.VSync = false;
    }

    protected override void OnLoad()
    {
        Random r = new Random();
        ParticleConsts[] particleConsts = new ParticleConsts[ParticleCount];
        PositionAndVelocity[] particleVars = new PositionAndVelocity[ParticleCount];
        for (int i = 0; i < ParticleCount; i++)
        {
            Vector2 position = r.RandomDirection2() * r.NextFloat(InnerContainerRadius + ParticleRadius, ContainerRadius - ParticleRadius);
            Vector2 velocity = r.RandomDirection2();
            particleConsts[i] = new ParticleConsts(ParticleMass, ParticleRadius);
            particleVars[i] = new PositionAndVelocity(position, velocity);
        }

        // particleVars[0] = new PositionAndVelocity();
        // particleConsts[0] = new ParticleConsts(10, ParticleRadius * 10);

        simulation = new ParticleSimulation(graphicsDevice, SimSizeX, SimSizeY, 3, ContainerRadius, particleConsts, particleVars);
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