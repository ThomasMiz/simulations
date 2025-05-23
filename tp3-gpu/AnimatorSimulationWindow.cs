﻿using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using SimulationBase;
using TrippyGL;

namespace tp3_gpu
{
    class AnimatorSimulationWindow : WindowBase
    {
        private readonly float animationSpeed;
        private readonly bool autoClose;
        private bool requestClose = false;

        private readonly SimulationConfig config;

        private Vector2 lastMousePos;
        private float mouseMoveScale;

        private Vector2 offset;
        private float scaleExponent;
        private float scale;

        private ParticleSimulation simulation;

        private BufferObject circleBuffer;
        private VertexDataBufferSubset<VertexPosition> circleSubset;
        private BufferObject particleColorsBuffer;
        private VertexDataBufferSubset<Color4b> particleColorsSubset;
        private VertexArray simulationDrawArray;
        private ShaderProgram simulationDrawProgram;

        private PrimitiveBatcher<VertexColor> primitiveBatcher;
        private VertexBuffer<VertexColor> primitiveBuffer;
        private SimpleShaderProgram primitiveProgram;

        private float simulationTime;
        private float lastStepTime;

        public AnimatorSimulationWindow(SimulationConfig config, float animationSpeed = 1, bool autoClose = false)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.animationSpeed = animationSpeed;
            this.autoClose = autoClose;

            Window.FramesPerSecond = 0;
            Window.VSync = false;
        }

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
            Color4b[] particleColors = new Color4b[config.ParticleCount];
            for (int i = 0; i < particleColors.Length; i++)
                particleColors[i] = Color4b.FromHSV(i / (float)particleColors.Length, 1, 1);
            particleColorsBuffer = new BufferObject(graphicsDevice, DataBufferSubset.CalculateRequiredSizeInBytes<Color4b>((uint)config.ParticleCount), BufferUsage.StaticDraw);
            particleColorsSubset = new VertexDataBufferSubset<Color4b>(particleColorsBuffer, particleColors);

            // Make simulation draw array
            ReadOnlySpan<VertexAttribSource> attribSources =
            [
                new VertexAttribSource(circleSubset, AttributeType.FloatVec3),
                new VertexAttribSource(particleColorsSubset, AttributeType.FloatVec4, true, AttributeBaseType.UnsignedByte, 1),
            ];

            simulationDrawArray = new VertexArray(graphicsDevice, attribSources);

            simulationDrawProgram = ShaderProgram.FromFiles<VertexColor>(graphicsDevice, "data/draw_sim_vs.glsl", "data/draw_sim_fs.glsl", "vPosition", "vColor");

            // Make primitive drawer
            primitiveBatcher = new PrimitiveBatcher<VertexColor>(1024, 128);
            primitiveBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)primitiveBatcher.TriangleVertexCapacity, BufferUsage.StreamDraw);
            primitiveProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice, 0, 0, true);

            simulation = new ParticleSimulation(graphicsDevice, config);

            OnKeyDown(null, Key.R, 0); // Simulation is initialized inside here
            OnKeyDown(null, Key.Home, 0);
        }

        protected override void OnRender(double dt)
        {
            if ((config.MaxSteps == null || simulation.Steps < config.MaxSteps) && (config.MaxSimulationTime == null || simulation.SecondsElapsed < config.MaxSimulationTime))
            {
                float deltaTime = (float)dt * animationSpeed;
                simulationTime += deltaTime;
                //while (simulationTime >= simulation.NextCollisionTime)
                if (simulationTime >= simulation.NextCollisionTime)
                {
                    simulationTime = simulation.NextCollisionTime;
                    lastStepTime = simulation.NextCollisionTime;
                    simulation.Step();
                    bool limitReached = false;
                    if (simulation.SecondsElapsed >= config.MaxSimulationTime)
                    {
                        Console.WriteLine($"Reached time limit of {simulation.SecondsElapsed} after {simulation.Steps} steps");
                        limitReached = true;
                    }
                    else if (simulation.Steps >= config.MaxSteps)
                    {
                        Console.WriteLine($"Reached step limit of {simulation.Steps} after simulating {simulation.SecondsElapsed} seconds");
                        limitReached = true;
                    }
                    else if (simulation.Steps % 1000 == 0)
                    {
                        Console.WriteLine($"Ran step {simulation.Steps} with next collision time {simulation.SecondsElapsed}");
                    }

                    if (limitReached && autoClose)
                    {
                        requestClose = true;
                    }
                }
            }

            graphicsDevice.Framebuffer = null;
            graphicsDevice.SetViewport(0, 0, (uint)Window.Size.X, (uint)Window.Size.Y);
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ClearColor = Color4b.Black;
            graphicsDevice.Clear(ClearBuffers.Color);

            primitiveBatcher.ClearTriangles();
            primitiveBatcher.ClearLines();
            primitiveBatcher.AddCirclePrecise(Vector2.Zero, config.ContainerRadius * 1.25f, Color4b.OrangeRed);
            primitiveBatcher.AddCirclePrecise(Vector2.Zero, config.ContainerRadius, new Color4b(64, 64, 64));
            uint minStorage = (uint)Math.Max(primitiveBatcher.TriangleVertexCount, primitiveBatcher.LineVertexCount);
            if (primitiveBuffer.StorageLength < minStorage)
                primitiveBuffer.RecreateStorage((uint)Math.Max(primitiveBatcher.TriangleVertexCapacity, primitiveBatcher.LineVertexCapacity));
            graphicsDevice.VertexArray = primitiveBuffer;
            graphicsDevice.ShaderProgram = primitiveProgram;
            primitiveBuffer.DataSubset.SetData(primitiveBatcher.LineVertices);
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, (uint)primitiveBatcher.LineVertexCount);
            primitiveBuffer.DataSubset.SetData(primitiveBatcher.TriangleVertices);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)primitiveBatcher.TriangleVertexCount);

            graphicsDevice.ShaderProgram = simulationDrawProgram;
            graphicsDevice.VertexArray = simulationDrawArray;
            simulationDrawProgram.Uniforms["constantsSampler"].SetValueTexture(simulation.ParticleConstsTexture);
            simulationDrawProgram.Uniforms["previousPosAndVelSampler"].SetValueTexture(simulation.ParticleVarsBuffers[0].PositionAndVelocity);
            simulationDrawProgram.Uniforms["timeSinceLastStep"].SetValueFloat(simulationTime - lastStepTime);
            graphicsDevice.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, circleSubset.StorageLength, simulation.ParticleCount);
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);
            if (requestClose && !Window.IsClosing)
            {
                Window.Close();
            }
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            Matrix4x4 projection;
            if (size.X < size.Y)
            {
                projection = Matrix4x4.CreateOrthographic(2f * size.X / size.Y, 2f, 0.01f, 10f);
                mouseMoveScale = 2f / size.Y;
            }
            else
            {
                projection = Matrix4x4.CreateOrthographic(2f, 2f * size.Y / size.X, 0.01f, 10f);
                mouseMoveScale = 2f / size.X;
            }

            simulationDrawProgram.Uniforms["projection"].SetValueMat4(projection);
            primitiveProgram.Projection = projection;
        }

        protected override void OnUnload()
        {
            simulationDrawProgram.Dispose();
            simulationDrawArray.Dispose();
            circleBuffer.Dispose();
            primitiveProgram.Dispose();
            primitiveBuffer.Dispose();
            particleColorsBuffer.Dispose();
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

                /*case Key.R:
                    simulation?.Dispose();

                    simulationTime = 0;
                    lastStepTime = 0;
                    ParticleConsts[] particleConsts = new ParticleConsts[ParticleCount];
                    PositionAndVelocity[] particleVars = new PositionAndVelocity[ParticleCount];
                    Color4b[] particleColors = new Color4b[ParticleCount];
                    for (int i = 0; i < ParticleCount; i++)
                    {
                        Vector2 position = r.RandomDirection2() * r.NextFloat(InnerContainerRadius + ParticleRadius, ContainerRadius - ParticleRadius);
                        Vector2 velocity = r.RandomDirection2();
                        particleConsts[i] = new ParticleConsts(ParticleMass, ParticleRadius);
                        particleVars[i] = new PositionAndVelocity(position, velocity);
                        particleColors[i] = Color4b.FromHSV(i / (float)ParticleCount, 1, 1);
                    }

                    simulation = new ParticleSimulation(graphicsDevice, SimSizeX, SimSizeY, 3, ContainerRadius, particleConsts, particleVars);
                    particleColorsSubset.SetData(particleColors);
                    break;*/
            }
        }

        private void UpdateTransformMatrix()
        {
            float s = 0.3f / config.ContainerRadius;
            Matrix4x4 view = Matrix4x4.CreateScale(s, s, 1f) * Matrix4x4.CreateTranslation(offset.X, offset.Y, 0) * Matrix4x4.CreateScale(scale);
            simulationDrawProgram.Uniforms["view"].SetValueMat4(view);
            primitiveProgram.View = view;
        }
    }
}