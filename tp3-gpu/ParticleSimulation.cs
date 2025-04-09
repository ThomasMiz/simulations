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

    private FramebufferObject currentParticleFramebuffer;
    private Texture2D currentParticleTexture;
    private FramebufferObject previousParticleFramebuffer;
    private Texture2D previousParticleTexture;

    private Framebuffer2D aggregationBuffer;

    private ShaderProgram simulationProgram;

    public Texture2D ParticleConstsTexture => particleConstsBuffer.Texture;
    public Texture2D ParticleVarsTexture => currentParticleTexture;
    public Texture2D PreviousVarsTexture => previousParticleTexture;

    public ParticleSimulation(GraphicsDevice graphicsDevice, uint sizeX, uint sizeY, ReadOnlySpan<ParticleConsts> particleConsts, ReadOnlySpan<ParticleVars> particleVars)
    {
        if (sizeX <= 0) throw new ArgumentOutOfRangeException(nameof(sizeX));
        if (sizeY <= 0) throw new ArgumentOutOfRangeException(nameof(sizeY));
        if (particleConsts.Length != sizeX * sizeY) throw new ArgumentException("particleConsts length must match the particle count (sizeX * sizeY)");
        if (particleVars.Length != sizeX * sizeY) throw new ArgumentException("particleVars length must match the particle count (sizeX * sizeY)");

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

        currentParticleTexture = new Texture2D(this.graphicsDevice, SizeX, SizeY, false, 0, TextureImageFormat.Float4);
        currentParticleTexture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        currentParticleTexture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        previousParticleTexture = new Texture2D(this.graphicsDevice, SizeX, SizeY, false, 0, TextureImageFormat.Float4);
        previousParticleTexture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        previousParticleTexture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        currentParticleFramebuffer = new FramebufferObject(graphicsDevice);
        currentParticleFramebuffer.Attach(currentParticleTexture, FramebufferAttachmentPoint.Color0);
        currentParticleFramebuffer.UpdateFramebufferData();

        previousParticleFramebuffer = new FramebufferObject(graphicsDevice);
        previousParticleFramebuffer.Attach(previousParticleTexture, FramebufferAttachmentPoint.Color0);
        previousParticleFramebuffer.UpdateFramebufferData();

        particleConstsBuffer.Texture.SetData(particleConsts);
        currentParticleTexture.SetData(particleVars);

        aggregationBuffer = new Framebuffer2D(graphicsDevice, sizeX, 1, DepthStencilFormat.None, 0, TextureImageFormat.Float);
        aggregationBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        aggregationBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        simulationProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "data/dum_vs.glsl", "data/sim_fs_particles.glsl", "vPosition");
    }

    public void Step()
    {
        (previousParticleFramebuffer, currentParticleFramebuffer) = (currentParticleFramebuffer, previousParticleFramebuffer);
        (previousParticleTexture, currentParticleTexture) = (currentParticleTexture, previousParticleTexture);

        graphicsDevice.Framebuffer = currentParticleFramebuffer;
        // graphicsDevice.GL.DrawBuffers([DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1]);
        graphicsDevice.SetViewport(0, 0, SizeX, SizeY);
        graphicsDevice.BlendingEnabled = false;
        graphicsDevice.DepthTestingEnabled = false;

        graphicsDevice.VertexArray = vertexBuffer;
        graphicsDevice.ShaderProgram = simulationProgram;
        simulationProgram.Uniforms["consts"].SetValueTexture(particleConstsBuffer);
        simulationProgram.Uniforms["previous"].SetValueTexture(previousParticleTexture);
        simulationProgram.Uniforms["deltaTime"].SetValueFloat(0.001f);
        simulationProgram.Uniforms["containerRadius"].SetValueFloat(0.1f);
        graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        simulationProgram.Dispose();
        particleConstsBuffer.Dispose();
        currentParticleFramebuffer.Dispose();
        currentParticleTexture.Dispose();
        previousParticleFramebuffer.Dispose();
        previousParticleTexture.Dispose();
        aggregationBuffer.Dispose();
    }
}