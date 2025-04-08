using System.Numerics;
using TrippyGL;

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
    private Framebuffer2D currentParticleBuffer, previousParticleBuffer;
    private Framebuffer2D aggregationBuffer;

    public Texture2D ParticleConstsBuffer => particleConstsBuffer.Texture;
    public Texture2D ParticleVarsBuffer => currentParticleBuffer.Texture;
    
    public ParticleSimulation(GraphicsDevice graphicsDevice, uint sizeX, uint sizeY, ReadOnlySpan<ParticleConsts> particleConsts, ReadOnlySpan<ParticleVars> particleVars)
    {
        if (sizeX <= 0) throw new ArgumentOutOfRangeException(nameof(sizeX));
        if (sizeY <= 0) throw new ArgumentOutOfRangeException(nameof(sizeY));
        if (particleConsts.Length != sizeX * sizeY) throw new ArgumentException("particleConsts length must match the particle count (sizeX * sizeY)");
        if (particleVars.Length != sizeX * sizeY) throw new ArgumentException("particleVars length must match the particle count (sizeX * sizeY)");

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
        currentParticleBuffer = new Framebuffer2D(graphicsDevice, sizeX, sizeY, DepthStencilFormat.None, 0, TextureImageFormat.Float4);
        previousParticleBuffer = new Framebuffer2D(graphicsDevice, sizeX, sizeY, DepthStencilFormat.None, 0, TextureImageFormat.Float4);
        particleConstsBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        currentParticleBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        previousParticleBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        particleConstsBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
        currentParticleBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
        previousParticleBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

        particleConstsBuffer.Texture.SetData(particleConsts);
        currentParticleBuffer.Texture.SetData(particleVars);

        aggregationBuffer = new Framebuffer2D(graphicsDevice, sizeX, 1, DepthStencilFormat.None, 0, TextureImageFormat.Float);
        aggregationBuffer.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        aggregationBuffer.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        particleConstsBuffer.Dispose();
        currentParticleBuffer.Dispose();
        previousParticleBuffer.Dispose();
        aggregationBuffer.Dispose();
    }
}