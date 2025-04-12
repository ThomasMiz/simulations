using System.Numerics;
using TrippyGL;
using BlendingFactor = TrippyGL.BlendingFactor;
using PrimitiveType = TrippyGL.PrimitiveType;
using TextureMagFilter = TrippyGL.TextureMagFilter;
using TextureMinFilter = TrippyGL.TextureMinFilter;
using TextureWrapMode = TrippyGL.TextureWrapMode;
using VertexArray = TrippyGL.VertexArray;

namespace tp3_gpu;

public class ParticleSimulation : IDisposable
{
    public uint SizeX { get; }
    public uint SizeY { get; }
    public float ContainerRadius { get; }

    public uint ParticleCount => SizeX * SizeY;

    public uint Steps { get; private set; } = 0;
    public float SecondsElapsed { get; private set; } = 0;

    public float TimeToNextCollision { get; private set; }
    public float NextCollisionTime => SecondsElapsed + TimeToNextCollision;

    private readonly GraphicsDevice graphicsDevice;

    private VertexBuffer<VertexPosition> vertexBuffer;

    private Framebuffer2D particleConstsBuffer;
    private List<ParticleVarsBuffer> particleVarsBuffers; // Index 0 is current, higher indexes are older.

    private Framebuffer2D aggregationBuffer;
    private VertexArray aggregationVertexArray;
    private ShaderProgram aggregationProgram;

    private ShaderProgram simulationAdvanceProgram;
    private ShaderProgram simulationCalctimeProgram;

    public Texture2D ParticleConstsTexture => particleConstsBuffer.Texture;
    public IReadOnlyList<ParticleVarsBuffer> ParticleVarsBuffers => particleVarsBuffers;

    private static readonly BlendState minBlendState = new BlendState(false, BlendingMode.Min, BlendingFactor.One, BlendingFactor.One);

    private SimulationFileSaver fileSaver;

    public ParticleSimulation(GraphicsDevice graphicsDevice, uint sizeX, uint sizeY, uint particleBuffersCount, float containerRadius, ReadOnlySpan<ParticleConsts> particleConsts, ReadOnlySpan<PositionAndVelocity> particleVars)
    {
        if (sizeX <= 0) throw new ArgumentOutOfRangeException(nameof(sizeX));
        if (sizeY <= 0) throw new ArgumentOutOfRangeException(nameof(sizeY));
        if (particleConsts.Length != sizeX * sizeY) throw new ArgumentException("particleConsts length must match the particle count (sizeX * sizeY)");
        if (particleVars.Length != sizeX * sizeY) throw new ArgumentException("particleVars length must match the particle count (sizeX * sizeY)");
        if (particleBuffersCount < 2) throw new ArgumentException(nameof(particleBuffersCount) + " must be at least 2");
        if (particleBuffersCount > 16) throw new ArgumentException("not gonna let you kill ur computer", nameof(particleBuffersCount));
        if (containerRadius <= 0) throw new ArgumentException("Container radius must be greater than 0", nameof(containerRadius));

        SizeX = sizeX;
        SizeY = sizeY;
        ContainerRadius = containerRadius;
        this.graphicsDevice = graphicsDevice;

        ReadOnlySpan<VertexPosition> vertexBufferData =
        [
            new VertexPosition(new Vector3(-1, -1, 0)),
            new VertexPosition(new Vector3(-1, 1, 0)),
            new VertexPosition(new Vector3(1, -1, 0)),
            new VertexPosition(new Vector3(1, 1, 0))
        ];

        // Create dummy vertex buffer
        vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexBufferData, BufferUsage.StaticDraw);

        // Create particle constants buffer
        particleConstsBuffer = new Framebuffer2D(graphicsDevice, sizeX, sizeY, DepthStencilFormat.None, 0, TextureImageFormat.Float2);
        particleConstsBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        particleConstsBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        particleVarsBuffers = new((int)particleBuffersCount);
        for (uint i = 0; i < particleBuffersCount; i++)
            particleVarsBuffers.Add(new ParticleVarsBuffer(graphicsDevice, sizeX, sizeY));

        // Create aggregation buffer, vertexarray and program
        aggregationBuffer = new Framebuffer2D(graphicsDevice, 1, 1, DepthStencilFormat.None, 0, TextureImageFormat.Float);
        aggregationBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        aggregationBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        aggregationVertexArray = new VertexArray(graphicsDevice, []);

        ShaderProgramBuilder aggregationProgramBuilder = new()
        {
            VertexShaderCode = File.ReadAllText("data/sim_vs_aggregation.glsl"),
            FragmentShaderCode = File.ReadAllText("data/sim_fs_aggregation.glsl"),
        };
        aggregationProgramBuilder.SpecifyVertexAttribs([]);
        aggregationProgram = aggregationProgramBuilder.Create(this.graphicsDevice);

        // Create simulation program
        simulationAdvanceProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "data/dum_vs.glsl", "data/sim_fs_advance.glsl", "vPosition");
        simulationCalctimeProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "data/dum_vs.glsl", "data/sim_fs_calctime.glsl", "vPosition");

        // Set particle constants (mass & radius) and variables (position & velocity)
        particleConstsBuffer.Texture.SetData(particleConsts);
        particleVarsBuffers[0].PositionAndVelocity.Texture.SetData(particleVars);

        TimeToCollisionAndCollidesWith[] tmpttcacw = new TimeToCollisionAndCollidesWith[ParticleCount];
        Array.Fill(tmpttcacw, new TimeToCollisionAndCollidesWith { TimeToCollision = -100 });
        particleVarsBuffers[0].TimeToCollisionAndCollidesWith.Texture.SetData<TimeToCollisionAndCollidesWith>(tmpttcacw);

        // Calculate the initial time-to-collision and with-who for all particles
        RecalculateMinTimeToCollision();

        // File saver :-)
        fileSaver = new SimulationFileSaver(containerRadius, particleConsts, "output.sim");
        fileSaver?.Save(Steps, SecondsElapsed, particleVarsBuffers[0]);
    }

    private void RecalculateMinTimeToCollision()
    {
        // Calculate the time to next collision for all particles and store it in TimeToCollisionAndCollidesWith
        graphicsDevice.Framebuffer = particleVarsBuffers[0].TimeToCollisionAndCollidesWith;
        graphicsDevice.SetViewport(0, 0, SizeX, SizeY);
        graphicsDevice.BlendingEnabled = false;
        graphicsDevice.DepthTestingEnabled = false;
        graphicsDevice.VertexArray = vertexBuffer;
        graphicsDevice.ShaderProgram = simulationCalctimeProgram;
        simulationCalctimeProgram.Uniforms["containerRadius"].SetValueFloat(ContainerRadius);
        simulationCalctimeProgram.Uniforms["constantsSampler"].SetValueTexture(particleConstsBuffer);
        simulationCalctimeProgram.Uniforms["posAndVelSampler"].SetValueTexture(particleVarsBuffers[0].PositionAndVelocity);
        graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

        graphicsDevice.Framebuffer = aggregationBuffer;
        graphicsDevice.SetViewport(0, 0, aggregationBuffer.Width, aggregationBuffer.Height);

        // Clear the aggregation buffer
        graphicsDevice.ClearColor = new Vector4(999999999999999.9f);
        graphicsDevice.Clear(ClearBuffers.Color);

        // Draw to the aggregation buffer with min-function blendstate
        graphicsDevice.BlendState = minBlendState;
        graphicsDevice.DepthTestingEnabled = false;
        graphicsDevice.VertexArray = aggregationVertexArray;
        graphicsDevice.ShaderProgram = aggregationProgram;
        aggregationProgram.Uniforms["timeToCollisionAndCollidesWithSampler"].SetValueTexture(particleVarsBuffers[0].TimeToCollisionAndCollidesWith);
        graphicsDevice.DrawArrays(PrimitiveType.Points, 0, ParticleCount);

        Span<float> miny = stackalloc float[1];
        aggregationBuffer.Texture.GetData(miny);
        TimeToNextCollision = miny[0];

        if (TimeToNextCollision <= 0)
        {
            Console.WriteLine("Warning: TimeToNextCollision is <= 0: {0}", TimeToNextCollision);
            TimeToNextCollision = 0.001f;
        }
    }

    public void Step()
    {
        // Console.WriteLine("Minimum time to collision: " + TimeToNextCollision);

        // PositionAndVelocity[] posandvel0 = new PositionAndVelocity[ParticleCount];
        // TimeToCollisionAndCollidesWith[] timetocol0 = new TimeToCollisionAndCollidesWith[ParticleCount];
        // particleVarsBuffers[0].PositionAndVelocity.Texture.GetData<PositionAndVelocity>(posandvel0);
        // particleVarsBuffers[0].TimeToCollisionAndCollidesWith.Texture.GetData<TimeToCollisionAndCollidesWith>(timetocol0);

        // Move the last buffer to the beginning of the list, we then draw on it so it becomes the current.
        ParticleVarsBuffer oldest = particleVarsBuffers[^1];
        particleVarsBuffers.RemoveAt(particleVarsBuffers.Count - 1);
        particleVarsBuffers.Insert(0, oldest);

        graphicsDevice.Framebuffer = particleVarsBuffers[0].PositionAndVelocity;
        graphicsDevice.SetViewport(0, 0, SizeX, SizeY);
        graphicsDevice.BlendingEnabled = false;
        graphicsDevice.DepthTestingEnabled = false;
        graphicsDevice.VertexArray = vertexBuffer;
        graphicsDevice.ShaderProgram = simulationAdvanceProgram;
        simulationAdvanceProgram.Uniforms["constantsSampler"].SetValueTexture(particleConstsBuffer);
        simulationAdvanceProgram.Uniforms["posAndVelSampler"].SetValueTexture(particleVarsBuffers[1].PositionAndVelocity);
        simulationAdvanceProgram.Uniforms["timeToCollisionAndCollidesWithSampler"].SetValueTexture(particleVarsBuffers[1].TimeToCollisionAndCollidesWith);
        simulationAdvanceProgram.Uniforms["deltaTime"].SetValueFloat(TimeToNextCollision);
        graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

        Steps++;
        SecondsElapsed += TimeToNextCollision;

        RecalculateMinTimeToCollision();

        // Save state to file
        fileSaver?.Save(Steps, SecondsElapsed, particleVarsBuffers[0]);

        // PositionAndVelocity[] posandvel1 = new PositionAndVelocity[ParticleCount];
        // TimeToCollisionAndCollidesWith[] timetocol1 = new TimeToCollisionAndCollidesWith[ParticleCount];
        // particleVarsBuffers[0].PositionAndVelocity.Texture.GetData<PositionAndVelocity>(posandvel1);
        // particleVarsBuffers[0].TimeToCollisionAndCollidesWith.Texture.GetData<TimeToCollisionAndCollidesWith>(timetocol1);
    }

    public void Dispose()
    {
        fileSaver?.Dispose();
        vertexBuffer.Dispose();
        simulationAdvanceProgram.Dispose();
        simulationCalctimeProgram.Dispose();
        particleConstsBuffer.Dispose();
        particleVarsBuffers.ForEach(particleVarsBuffer => particleVarsBuffer.Dispose());
        aggregationBuffer.Dispose();
    }
}

public struct ParticleVarsBuffer : IDisposable
{
    public Framebuffer2D PositionAndVelocity { get; }
    public Framebuffer2D TimeToCollisionAndCollidesWith { get; }

    public ParticleVarsBuffer(GraphicsDevice graphicsDevice, uint sizeX, uint sizeY)
    {
        PositionAndVelocity = new Framebuffer2D(graphicsDevice, sizeX, sizeY, DepthStencilFormat.None, 0, TextureImageFormat.Float4);
        PositionAndVelocity.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        PositionAndVelocity.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        TimeToCollisionAndCollidesWith = new Framebuffer2D(graphicsDevice, sizeX, sizeY, DepthStencilFormat.None, 0, TextureImageFormat.Float3);
        TimeToCollisionAndCollidesWith.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        TimeToCollisionAndCollidesWith.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
    }

    public void Dispose()
    {
        PositionAndVelocity.Dispose();
        TimeToCollisionAndCollidesWith.Dispose();
    }
}