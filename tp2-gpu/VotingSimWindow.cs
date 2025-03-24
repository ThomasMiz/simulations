using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using SimulationBase;
using TrippyGL;
using TrippyGL.Utils;

namespace tp2
{
    class VotingSimWindow : WindowBase
    {
        const uint SimulationWidth = 50, SimulationHeight = 50;
        const float P = 0.01f;

        VertexBuffer<VertexTexture> vertexBuffer;

        Framebuffer2D fbo1, fbo2, fboTmp;
        float[] sumTmpArray = new float[SimulationWidth];

        ShaderProgram simProgram;
        ShaderUniform simPrevUniform;

        ShaderProgram sumProgram;

        SimpleShaderProgram drawProgram;

        Vector2 lastMousePos;
        float mouseMoveScale;

        Vector2 offset;
        float scaleExponent;
        float scale;

        int steps = 0;
        int randomSeed = 124;

        private StreamWriter fileWriter = new StreamWriter("output.txt");

        public VotingSimWindow() : base()
        {
            Window.FramesPerSecond = -1;
        }

        protected override void OnLoad()
        {
            Span<VertexTexture> vertices = stackalloc VertexTexture[]
            {
                new VertexTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                new VertexTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
            };

            vertexBuffer = new VertexBuffer<VertexTexture>(graphicsDevice, vertices, BufferUsage.StaticDraw);

            fbo1 = new Framebuffer2D(graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None); //FramebufferObject.Create2D(ref tex1, graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);
            fbo2 = new Framebuffer2D(graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None); //FramebufferObject.Create2D(ref tex2, graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);
            fboTmp = new Framebuffer2D(graphicsDevice, SimulationWidth, 1, DepthStencilFormat.None, 0, TextureImageFormat.Float);

            fbo1.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            fbo2.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            fbo1.Texture.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            fbo2.Texture.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            fboTmp.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            fboTmp.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            simProgram = ShaderProgram.FromFiles<VertexTexture>(graphicsDevice, "data/sim_vs.glsl", "data/sim_fs_voting.glsl", new string[] { "vPosition", "vTexCoords" });
            drawProgram = SimpleShaderProgram.Create<VertexTexture>(graphicsDevice);

            simProgram.Uniforms["pixelDelta"].SetValueVec2(1f / SimulationWidth, 1f / SimulationHeight);
            simPrevUniform = simProgram.Uniforms["previous"];

            simProgram.Uniforms["p"].SetValueFloat(P);

            sumProgram = ShaderProgram.FromFiles<VertexTexture>(graphicsDevice, "data/sim_vs.glsl", "data/fs_column_sum.glsl", new string[] { "vPosition", "vTexCoords" });

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            OnKeyDown(null, Key.Home, 0);
            OnKeyDown(null, Key.R, 0);
        }

        private int xd = 1;

        protected override void OnRender(double dt)
        {
            graphicsDevice.VertexArray = vertexBuffer;

            if (InputContext.Keyboards[0].IsKeyPressed(Key.Space))
            {
                if ((xd = (xd + 1) % 5) == 0 || InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
                {
                    graphicsDevice.Framebuffer = fbo2;
                    graphicsDevice.SetViewport(0, 0, fbo1.Width, fbo1.Height);
                    graphicsDevice.ShaderProgram = simProgram;
                    simPrevUniform.SetValueTexture(fbo1);
                    simProgram.Uniforms["time"].SetValueFloat((float)Window.Time);
                    graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

                    graphicsDevice.Framebuffer = fboTmp;
                    graphicsDevice.SetViewport(0, 0, fboTmp.Width, fboTmp.Height);
                    graphicsDevice.ShaderProgram = sumProgram;
                    sumProgram.Uniforms["data"].SetValueTexture(fbo2);
                    graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

                    fboTmp.Texture.GetData<float>(sumTmpArray);
                    float m = MathF.Abs(sumTmpArray.Sum()) / (SimulationWidth * SimulationHeight);

                    steps++;
                    (fbo1, fbo2) = (fbo2, fbo1);

                    fileWriter.WriteLine("{0},{1}", steps, m);
                }
            }
            else
            {
                xd = -1;
            }

            graphicsDevice.Framebuffer = null;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBuffers.Color);
            graphicsDevice.SetViewport(0, 0, (uint)Window.Size.X, (uint)Window.Size.Y);
            graphicsDevice.ShaderProgram = drawProgram;
            drawProgram.Texture = fbo1;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            if (size.X < size.Y)
            {
                drawProgram.Projection = Matrix4x4.CreateOrthographic(2f * size.X / size.Y, 2f, 0.01f, 10f);
                mouseMoveScale = 2f / size.Y;
            }
            else
            {
                drawProgram.Projection = Matrix4x4.CreateOrthographic(2f, 2f * size.Y / size.X, 0.01f, 10f);
                mouseMoveScale = 2f / size.X;
            }
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            fbo1.Dispose();
            fbo2.Dispose();
            fboTmp.Dispose();
            simProgram.Dispose();
            drawProgram.Dispose();

            fileWriter.Flush();
            fileWriter.Close();
        }

        protected override void OnMouseMove(IMouse sender, Vector2 position)
        {
            if (Window.IsClosing)
                return;

            if (sender.IsButtonPressed(MouseButton.Left))
            {
                offset.X += (position.X - lastMousePos.X) * mouseMoveScale / scale;
                offset.Y += (lastMousePos.Y - position.Y) * mouseMoveScale / scale;
                lastMousePos = position;
                UpdateTransformMatrix();
            }
        }

        protected override void OnMouseDown(IMouse sender, MouseButton button)
        {
            if (button == MouseButton.Left)
                lastMousePos = sender.Position;
        }

        protected override void OnMouseScroll(IMouse sender, ScrollWheel scroll)
        {
            if (Window.IsClosing)
                return;

            scaleExponent = Math.Clamp(scaleExponent + scroll.Y * 0.05f, -5.5f, 1.0f);
            scale = MathF.Pow(10, scaleExponent);
            UpdateTransformMatrix();
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int n)
        {
            if (Window.IsClosing)
                return;

            switch (key)
            {
                case Key.Home:
                    offset = Vector2.Zero;
                    scaleExponent = 0.2f;
                    scale = MathF.Pow(10, scaleExponent);
                    UpdateTransformMatrix();
                    break;

                case Key.R:
                    Color4b[] noise = new Color4b[fbo1.Width * fbo1.Height];
                    Random r = new Random(randomSeed);
                    for (int i = 0; i < noise.Length; i++)
                        noise[i] = r.NextBool() ? Color4b.Black : Color4b.White;
                    fbo1.Texture.SetData<Color4b>(noise);
                    randomSeed++;
                    break;
            }
        }

        private void UpdateTransformMatrix()
        {
            Matrix4x4 mat = Matrix4x4.CreateScale(SimulationWidth / (float)SimulationHeight, 1f, 1f) * Matrix4x4.CreateTranslation(offset.X, offset.Y, 0) * Matrix4x4.CreateScale(scale);
            drawProgram.SetView(mat, Vector3.Zero);
        }
    }
}