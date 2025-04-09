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

    private ShaderProgram simulationProgram;

    public Texture2D ParticleConstsBuffer => particleConstsBuffer.Texture;
    public Texture2D ParticleVarsBuffer => currentParticleBuffer.Texture;
    
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
        
        simulationProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "data/dum_vs.glsl", "data/sim_fs_particles.glsl", "vPosition");
    }

    public void Step()
    {
        ParticleVars[] curr0 = new ParticleVars[ParticleCount];
        ParticleVars[] prev0 = new ParticleVars[ParticleCount];
        previousParticleBuffer.Texture.GetData<ParticleVars>(prev0);
        currentParticleBuffer.Texture.GetData<ParticleVars>(curr0);
        
        
        (previousParticleBuffer, currentParticleBuffer) = (currentParticleBuffer, previousParticleBuffer);
        
        graphicsDevice.Framebuffer = currentParticleBuffer;
        graphicsDevice.SetViewport(0, 0, SizeX, SizeY);
        graphicsDevice.BlendingEnabled = false;
        graphicsDevice.DepthTestingEnabled = false;
        
        graphicsDevice.VertexArray = vertexBuffer;
        graphicsDevice.ShaderProgram = simulationProgram;
        simulationProgram.Uniforms["consts"].SetValueTexture(particleConstsBuffer);
        simulationProgram.Uniforms["previous"].SetValueTexture(previousParticleBuffer);
        simulationProgram.Uniforms["deltaTime"].SetValueFloat(0.001f);
        graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
        
        ParticleVars[] curr = new ParticleVars[ParticleCount];
        ParticleVars[] prev = new ParticleVars[ParticleCount];
        previousParticleBuffer.Texture.GetData<ParticleVars>(prev);
        currentParticleBuffer.Texture.GetData<ParticleVars>(curr);
        
        Console.WriteLine("sad. :(");
    }
    
    public void Dispose()
    {
        vertexBuffer.Dispose();
        simulationProgram.Dispose();
        particleConstsBuffer.Dispose();
        currentParticleBuffer.Dispose();
        previousParticleBuffer.Dispose();
        aggregationBuffer.Dispose();
    }
}