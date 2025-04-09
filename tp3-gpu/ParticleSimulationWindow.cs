using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using SimulationBase;
using tp3_gpu;
using TrippyGL;
using TrippyGL.Utils;

namespace tp2
{
    class ParticleSimulationWindow : WindowBase
    {
        const int simSizeX = 100, simSizeY = 300;
        const int particleCount = simSizeX * simSizeY;

        private readonly Vector2 simulationAreaMin = new Vector2(-0.1f, -0.1f);
        private readonly Vector2 simulationAreaMax = new Vector2(0.1f, 0.1f);

        private Vector2 lastMousePos;
        private float mouseMoveScale;

        private Vector2 offset;
        private float scaleExponent;
        private float scale;

        private Random r = new Random();

        private ParticleSimulation simulation;

        private BufferObject circleBuffer;
        private VertexDataBufferSubset<VertexPosition> circleSubset;
        private BufferObject particleColorsBuffer;
        private VertexDataBufferSubset<Color4b> particleColorsSubset;
        private VertexArray simulationDrawArray;
        private ShaderProgram simulationDrawProgram;

        protected override void OnLoad()
        {
            // Make circle buffer
            Vector2[] circle2d = PrimitiveBatcherExtensions.generateCircle(6);
            VertexPosition[] circleVertices = new VertexPosition[circle2d.Length];
            for (int i = 0; i < circle2d.Length; i++)
                circleVertices[i] = new VertexPosition(new Vector3(circle2d[i], 0));

            circleBuffer = new BufferObject(graphicsDevice, DataBufferSubset.CalculateRequiredSizeInBytes<VertexPosition>((uint)circleVertices.Length), BufferUsage.StaticDraw);
            circleSubset = new VertexDataBufferSubset<VertexPosition>(circleBuffer, circleVertices);

            // Make particle colors buffer
            particleColorsBuffer = new BufferObject(graphicsDevice, DataBufferSubset.CalculateRequiredSizeInBytes<Color4b>(particleCount), BufferUsage.StaticDraw);
            particleColorsSubset = new VertexDataBufferSubset<Color4b>(particleColorsBuffer);

            // Make simulation draw array
            ReadOnlySpan<VertexAttribSource> attribSources =
            [
                new VertexAttribSource(circleSubset, AttributeType.FloatVec3),
                new VertexAttribSource(particleColorsSubset, AttributeType.FloatVec4, true, AttributeBaseType.UnsignedByte, 1),
            ];

            simulationDrawArray = new VertexArray(graphicsDevice, attribSources);

            simulationDrawProgram = ShaderProgram.FromFiles<VertexColor>(graphicsDevice, "data/draw_sim_vs.glsl", "data/draw_sim_fs.glsl", "vPosition", "vColor");

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            OnKeyDown(null, Key.R, 0); // Simulation is initialized inside here
            OnKeyDown(null, Key.Home, 0);
        }

        protected override void OnRender(double dt)
        {
            float deltaTime = (float)dt;
            simulation.Step();
            
            graphicsDevice.Framebuffer = null;
            graphicsDevice.ClearColor = Color4b.Black;
            graphicsDevice.Clear(ClearBuffers.Color);
            graphicsDevice.SetViewport(0, 0, (uint)Window.Size.X, (uint)Window.Size.Y);
            graphicsDevice.ShaderProgram = simulationDrawProgram;
            graphicsDevice.VertexArray = simulationDrawArray;
            //simulationDrawProgram.Uniforms["view"].SetValueMat4(Matrix4x4.Identity);
            //simulationDrawProgram.Uniforms["projection"].SetValueMat4(Matrix4x4.Identity);
            simulationDrawProgram.Uniforms["consts"].SetValueTexture(simulation.ParticleConstsBuffer);
            simulationDrawProgram.Uniforms["particles"].SetValueTexture(simulation.ParticleVarsBuffer);
            graphicsDevice.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, circleSubset.StorageLength, particleCount);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            if (size.X < size.Y)
            {
                simulationDrawProgram.Uniforms["projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f * size.X / size.Y, 2f, 0.01f, 10f));
                mouseMoveScale = 2f / size.Y;
            }
            else
            {
                simulationDrawProgram.Uniforms["projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f, 2f * size.Y / size.X, 0.01f, 10f));
                mouseMoveScale = 2f / size.X;
            }
        }

        protected override void OnUnload()
        {
            circleBuffer.Dispose();
            particleColorsBuffer.Dispose();
            simulationDrawArray.Dispose();
            simulationDrawProgram.Dispose();
            simulation.Dispose();
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
                    simulation?.Dispose();

                    ParticleConsts[] particleConsts = new ParticleConsts[particleCount];
                    ParticleVars[] particleVars = new ParticleVars[particleCount];
                    Color4b[] particleColors = new Color4b[particleCount];
                    for (int i = 0; i < particleCount; i++)
                    {
                        float mass = 1;
                        float radius = 0.0005f;
                        Vector2 position = r.RandomDirection2() * r.NextFloat(0.005f + radius, 0.05f - radius);
                        Vector2 velocity = r.RandomDirection2();
                        particleConsts[i] = new ParticleConsts(mass, radius);
                        particleVars[i] = new ParticleVars(position, velocity);
                        particleColors[i] = Color4b.FromHSV(i / (float)particleCount, 1, 1);
                    }

                    simulation = new ParticleSimulation(graphicsDevice, simSizeX, simSizeY, particleConsts, particleVars);
                    particleColorsSubset.SetData(particleColors);
                    break;

                case Key.G:
                    ParticleConsts[] consts = new ParticleConsts[particleCount];
                    ParticleVars[] vars = new ParticleVars[particleCount];
                    simulation.ParticleConstsBuffer.GetData<ParticleConsts>(consts);
                    simulation.ParticleVarsBuffer.GetData<ParticleVars>(vars);
                    break;
            }
        }

        private void UpdateTransformMatrix()
        {
            Matrix4x4 mat = Matrix4x4.CreateScale((simulationAreaMax.X - simulationAreaMin.X) / (simulationAreaMax.Y - simulationAreaMin.Y), 1f, 1f) * Matrix4x4.CreateTranslation(offset.X, offset.Y, 0) * Matrix4x4.CreateScale(scale);
            simulationDrawProgram.Uniforms["view"].SetValueMat4(mat);
        }
    }
}