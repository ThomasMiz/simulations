using System.Numerics;
using Silk.NET.OpenGL;
using TrippyGL;
using PrimitiveType = TrippyGL.PrimitiveType;
using TextureMagFilter = TrippyGL.TextureMagFilter;
using TextureMinFilter = TrippyGL.TextureMinFilter;
using TextureWrapMode = TrippyGL.TextureWrapMode;

namespace tp3_gpu;

public class ParticleSimulation : IDisposable
{
    public uint SizeX { get; }
    public uint SizeY { get; }

    public uint ParticleCount => SizeX * SizeY;

    public uint Steps { get; private set; } = 0;
    public float SecondsElapsed { get; private set; } = 0;

    private readonly GraphicsDevice graphicsDevice;

    private VertexBuffer<VertexPosition> vertexBuffer;

    private Framebuffer2D particleConstsBuffer;
    private List<ParticleVarsBuffer> particleVarsBuffers; // Index 0 is current, higher indexes are older.

    private Framebuffer2D aggregationBuffer;

    private ShaderProgram simulationProgram;

    public Texture2D ParticleConstsTexture => particleConstsBuffer.Texture;
    public IReadOnlyList<ParticleVarsBuffer> ParticleVarsBuffers => particleVarsBuffers;
    
    public ParticleSimulation(GraphicsDevice graphicsDevice, uint sizeX, uint sizeY, uint particleBuffersCount, ReadOnlySpan<ParticleConsts> particleConsts, ReadOnlySpan<PositionAndVelocity> particleVars)
    {
        if (sizeX <= 0) throw new ArgumentOutOfRangeException(nameof(sizeX));
        if (sizeY <= 0) throw new ArgumentOutOfRangeException(nameof(sizeY));
        if (particleConsts.Length != sizeX * sizeY) throw new ArgumentException("particleConsts length must match the particle count (sizeX * sizeY)");
        if (particleVars.Length != sizeX * sizeY) throw new ArgumentException("particleVars length must match the particle count (sizeX * sizeY)");
        if (particleBuffersCount < 2) throw new ArgumentException(nameof(particleBuffersCount) + " must be at least 2");
        if (particleBuffersCount > 16) throw new ArgumentException("not gonna let you kill ur computer", nameof(particleBuffersCount));
        
        SizeX = sizeX;
        SizeY = sizeY;
        this.graphicsDevice = graphicsDevice;

        ReadOnlySpan<VertexPosition> vertexBufferData =
        [
            new VertexPosition(new Vector3(-1, -1, 0)),
            new VertexPosition(new Vector3(-1, 1, 0)),
            new VertexPosition(new Vector3(1, -1, 0)),
            new VertexPosition(new Vector3(1, 1, 0))
        ];

        vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexBufferData, BufferUsage.StaticDraw);

        particleConstsBuffer = new Framebuffer2D(graphicsDevice, sizeX, sizeY, DepthStencilFormat.None, 0, TextureImageFormat.Float2);
        particleConstsBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        particleConstsBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        particleVarsBuffers = new((int)particleBuffersCount);
        for (uint i = 0; i < particleBuffersCount; i++)
        {
            particleVarsBuffers.Add(new ParticleVarsBuffer(graphicsDevice, sizeX, sizeY));
        }

        particleConstsBuffer.Texture.SetData(particleConsts);
        particleVarsBuffers[0].PositionAndVelocity.SetData(particleVars);

        aggregationBuffer = new Framebuffer2D(graphicsDevice, sizeX, 1, DepthStencilFormat.None, 0, TextureImageFormat.Float);
        aggregationBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        aggregationBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        simulationProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "data/dum_vs.glsl", "data/sim_fs_particles.glsl", "vPosition");
    }

    public void Step()
    {
        // PositionAndVelocity[] posandvel0 = new PositionAndVelocity[ParticleCount];
        // TimeToCollisionAndCollidesWith[] timetocol0 = new TimeToCollisionAndCollidesWith[ParticleCount];
        // particleVarsBuffers[0].PositionAndVelocity.GetData<PositionAndVelocity>(posandvel0);
        // particleVarsBuffers[0].TimeToCollisionAndCollidesWith.GetData<TimeToCollisionAndCollidesWith>(timetocol0);
        
        // Move the last buffer to the beginning of the list, we then draw on it so it becomes the current.
        ParticleVarsBuffer oldest = particleVarsBuffers[^1];
        particleVarsBuffers.RemoveAt(particleVarsBuffers.Count - 1);
        particleVarsBuffers.Insert(0, oldest);
        
        graphicsDevice.Framebuffer = particleVarsBuffers[0].Framebuffer;
        graphicsDevice.SetViewport(0, 0, SizeX, SizeY);
        graphicsDevice.BlendingEnabled = false;
        graphicsDevice.DepthTestingEnabled = false;

        graphicsDevice.VertexArray = vertexBuffer;
        graphicsDevice.ShaderProgram = simulationProgram;
        simulationProgram.Uniforms["constantsSampler"].SetValueTexture(particleConstsBuffer);
        simulationProgram.Uniforms["posAndVelSampler"].SetValueTexture(particleVarsBuffers[1].PositionAndVelocity);
        simulationProgram.Uniforms["timeToCollisionAndCollidesWithSampler"].SetValueTexture(particleVarsBuffers[1].TimeToCollisionAndCollidesWith);
        simulationProgram.Uniforms["deltaTime"].SetValueFloat(0.001f);
        simulationProgram.Uniforms["containerRadius"].SetValueFloat(0.1f);
        graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
        
        // PositionAndVelocity[] posandvel1 = new PositionAndVelocity[ParticleCount];
        // TimeToCollisionAndCollidesWith[] timetocol1 = new TimeToCollisionAndCollidesWith[ParticleCount];
        // particleVarsBuffers[0].PositionAndVelocity.GetData<PositionAndVelocity>(posandvel1);
        // particleVarsBuffers[0].TimeToCollisionAndCollidesWith.GetData<TimeToCollisionAndCollidesWith>(timetocol1);
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        simulationProgram.Dispose();
        particleConstsBuffer.Dispose();
        particleVarsBuffers.ForEach(particleVarsBuffer => particleVarsBuffer.Dispose());
        aggregationBuffer.Dispose();
    }
}

public struct ParticleVarsBuffer : IDisposable
{
    public FramebufferObject Framebuffer { get; }
    public Texture2D PositionAndVelocity { get; }
    public Texture2D TimeToCollisionAndCollidesWith { get; }

    public ParticleVarsBuffer(GraphicsDevice graphicsDevice, uint sizeX, uint sizeY)
    {
        PositionAndVelocity = new Texture2D(graphicsDevice, sizeX, sizeY, false, 0, TextureImageFormat.Float4);
        PositionAndVelocity.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        PositionAndVelocity.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
        
        TimeToCollisionAndCollidesWith = new Texture2D(graphicsDevice, sizeX, sizeY, false, 0, TextureImageFormat.Float3);
        TimeToCollisionAndCollidesWith.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        TimeToCollisionAndCollidesWith.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        Framebuffer = new FramebufferObject(graphicsDevice);
        graphicsDevice.Framebuffer = Framebuffer;
        Framebuffer.Attach(PositionAndVelocity, FramebufferAttachmentPoint.Color0);
        Framebuffer.Attach(TimeToCollisionAndCollidesWith, FramebufferAttachmentPoint.Color1);
        Framebuffer.UpdateFramebufferData();
        graphicsDevice.GL.DrawBuffers([DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1]);
    }

    public void Dispose()
    {
        Framebuffer.Dispose();
        PositionAndVelocity.Dispose();
        TimeToCollisionAndCollidesWith.Dispose();
    }
}