using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using SimulationBase;
using tp5.Particles;
using TrippyGL;
using TrippyGL.Fonts.Extensions;

namespace tp5.DebugView;

public class SimulationWindow : WindowBase
{
    public Simulation Simulation { get; }

    public bool DynamicBounds { get; set; } = false;
    public Bounds? CustomSimulationBounds { get; set; } = null;

    public double SimulationSpeed { get; set; } = 1;
    private double simulationTargetTime = 0;

    public TextureFont Arial16Regular { get; private set; }

    private DebugRenderer debugRenderer;

    public SimulationWindow(Simulation simulation, Bounds? customSimulationBounds = null)
    {
        Simulation = simulation;
        CustomSimulationBounds = customSimulationBounds;
    }

    protected override void OnLoad()
    {
        Arial16Regular = TextureFontExtensions.FromFile(graphicsDevice, "Content/arial16regular.tglf")[0];
        // Arial16Regular.Texture.SaveAsImage("Arial16Regular.png", SaveImageFormat.Png);

        debugRenderer = new DebugRenderer(Simulation, graphicsDevice, Arial16Regular)
        {
            BlendState = BlendState.NonPremultiplied,
        };
        
        Window.UpdatesPerSecond = SimulationSpeed / Simulation.DeltaTime;
    }

    protected override void OnUpdate(double dt)
    {
        base.OnUpdate(dt);
        if (Window.Time < 3) return;

        if (!Simulation.HasStopped)
        {
            simulationTargetTime += dt * SimulationSpeed;
        }

        if (!Simulation.HasStopped && Simulation.SecondsElapsed < simulationTargetTime)
        {
            Simulation.Step();

            if (Simulation.Steps % 1000 == 0)
            {
                Console.WriteLine("Simulated step {0}", Simulation.Steps);
            }
        }
    }

    private (Vector2, Vector2) CalculateSimulationBounds()
    {
        Particle first = Simulation.Particles.First.Value;
        double minX = first.Position.X - first.Radius;
        double minY = first.Position.Y - first.Radius;
        double maxX = first.Position.X + first.Radius;
        double maxY = first.Position.Y + first.Radius;

        foreach (Particle p in Simulation.Particles)
        {
            minX = Math.Min(minX, p.Position.X - p.Radius);
            minY = Math.Min(minY, p.Position.Y - p.Radius);
            maxX = Math.Min(maxX, p.Position.X + p.Radius);
            maxY = Math.Min(maxY, p.Position.Y + p.Radius);
        }

        return (new Vector2((float)minX, (float)minY), new Vector2((float)maxX, (float)maxY));
    }

    protected override void OnRender(double dt)
    {
        Vector2D<int> windowSize = Window.Size;

        (Vector2, Vector2) border;
        if (DynamicBounds)
        {
            border = CalculateSimulationBounds();
        }
        else
        {
            Bounds b = CustomSimulationBounds ?? Simulation.Bounds;
            border = (new Vector2((float)b.Left, (float)b.Bottom), new Vector2((float)b.Right, (float)b.Top));
        }

        float borderWidth = border.Item2.X - border.Item1.X;
        float borderHeight = border.Item2.Y - border.Item1.Y;
        Vector2 borderCenter = (border.Item1 + border.Item2) * 0.5f;

        float s = Math.Min(windowSize.Y / borderHeight, windowSize.X / borderWidth) * 0.95f;
        Matrix4x4 view = Matrix4x4.CreateTranslation(-borderCenter.X, -borderCenter.Y, 0) * Matrix4x4.CreateScale(s, -s, 1);
        view *= Matrix4x4.CreateTranslation(windowSize.X / 2f, windowSize.Y / 2f, 0);
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, windowSize.X, windowSize.Y, 0, 0, 1);

        debugRenderer.View = view;
        debugRenderer.Projection = projection;
        debugRenderer.GridMinimumCoords = border.Item1 * 1.05f;
        debugRenderer.GridMaximumCoords = border.Item2 * 1.05f;
        debugRenderer.DebugPanelPosition = new Vector2(10, 10);

        graphicsDevice.Viewport = new Viewport(0, 0, (uint)windowSize.X, (uint)windowSize.Y);
        graphicsDevice.DepthTestingEnabled = false;
        graphicsDevice.ClearColor = Color4b.Black;
        graphicsDevice.Clear(ClearBuffers.Color);

        debugRenderer.RenderDebugData();
    }

    protected override void OnKeyDown(IKeyboard sender, Key key, int n)
    {
        // Resume simulation if Simulation.HasStopped
        if (Simulation.HasStopped && sender.IsKeyPressed(Key.C) && sender.IsKeyPressed(Key.V))
        {
            Console.WriteLine("Resuming simulation");
            if (Simulation.Steps >= Simulation.MaxSteps) Simulation.MaxSteps *= 2;
            if (Simulation.Particles.Count >= Simulation.MaxParticles) Simulation.MaxParticles += 20;
            Simulation.HasStopped = false;
            Simulation.NoSpace = false;
        }
    }

    protected override void OnResized(Vector2D<int> size)
    {
    }

    protected override void OnUnload()
    {
        debugRenderer.Dispose();
        Arial16Regular.Dispose();
    }
}