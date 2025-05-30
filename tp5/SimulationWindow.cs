using System.Numerics;
using Silk.NET.Maths;
using SimulationBase;
using tp5.PhysicsDebug;
using TrippyGL;
using TrippyGL.Fonts.Extensions;

namespace tp5;

public class SimulationWindow : WindowBase
{
    public Simulation Simulation { get; }
    public TextureFont Arial16Regular { get; private set; }

    private DebugView debugView;

    public SimulationWindow(Simulation simulation)
    {
        Simulation = simulation;
    }

    protected override void OnLoad()
    {
        Arial16Regular = TextureFontExtensions.FromFile(graphicsDevice, "Content/arial16regular.tglf")[0];
        // Arial16Regular.Texture.SaveAsImage("Arial16Regular.png", SaveImageFormat.Png);

        debugView = new DebugView(Simulation.PhysicsWorld, graphicsDevice, Arial16Regular)
        {
            BlendState = BlendState.NonPremultiplied,
            Enabled = true,
            Flags = DebugViewFlags.Joint | DebugViewFlags.ContactPoints | DebugViewFlags.PolygonPoints | DebugViewFlags.ContactNormals | DebugViewFlags.Shape | DebugViewFlags.Grid,
            GridMinimumCoords = Simulation.Bounds.BottomLeft * 1.05f,
            GridMaximumCoords = Simulation.Bounds.TopRight * 1.05f,
        };
    }

    protected override void OnUpdate(double dt)
    {
        base.OnUpdate(dt);

        while (!Simulation.HasStopped && Simulation.SecondsElapsed < Window.Time)
        {
            Simulation.Step();

            if (Simulation.Steps % 1000 == 0)
            {
                Console.WriteLine("Simulated step {0}", Simulation.Steps);
            }
        }
    }

    protected override void OnRender(double dt)
    {
        graphicsDevice.Viewport = new Viewport(0, 0, (uint)Window.Size.X, (uint)Window.Size.Y);
        graphicsDevice.DepthTestingEnabled = false;
        graphicsDevice.ClearColor = Color4b.Black;
        graphicsDevice.Clear(ClearBuffers.Color);
        
        debugView.RenderDebugData();
    }

    protected override void OnResized(Vector2D<int> size)
    {
        Rectangle border = Simulation.Bounds;

        float s = Math.Min(size.Y / border.Height, size.X / border.Width) * 0.95f;
        Matrix4x4 view = Matrix4x4.CreateTranslation(-border.Center.X / 2f, -border.Center.Y / 2f, 0) * Matrix4x4.CreateScale(s, -s, 1);
        view *= Matrix4x4.CreateTranslation(size.X / 2f, size.Y / 2f, 0);
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, 0, 1);

        debugView.View = view;
        debugView.Projection = projection;
    }

    protected override void OnUnload()
    {
        debugView.Dispose();
        Arial16Regular.Dispose();
    }
}