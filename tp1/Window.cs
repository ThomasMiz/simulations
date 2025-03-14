using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using SimulationBase;
using tp1.Simulate;
using TrippyGL;

namespace tp1
{
    class Window : WindowBase
    {
        private Simulation simulation;
        private int M;
        private PrimitiveBatcher<VertexColor> primitiveBatcher;
        private VertexBuffer<VertexColor> vertexBuffer;
        private SimpleShaderProgram shaderProgram;

        public Window()
        {
            resetSimulation();
        }

        private void resetSimulation()
        {
            var config = SimulationConfig.FromFile("Examples/Static100.txt");
            config.M = 16;
            simulation = new Simulation(config);
            simulation.Initialize();
        }

        protected override void OnLoad()
        {
            primitiveBatcher = new PrimitiveBatcher<VertexColor>(65536, 8192);

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)primitiveBatcher.TriangleVertexCapacity, BufferUsage.DynamicDraw);
            shaderProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            graphicsDevice.ClearColor = Color4b.White;
            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthTestingEnabled = false;
        }

        protected override void OnRender(double dt)
        {
            simulation.Step();
            Vector2 simSize = simulation.Size;
            Vector2 binSize = simulation.Neighbors.BinSize;

            graphicsDevice.Clear(ClearBuffers.Color);

            primitiveBatcher.ClearLines();
            primitiveBatcher.ClearTriangles();

            Particle selected = simulation.Particles.First();
            HashSet<Particle> selectedNeighbors = simulation.NeighborsDictionary[selected];

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    primitiveBatcher.AddCirclePrecise(selected.Position + new Vector2(x, y) * simSize, selected.Radius + simulation.NeighborRadius, new Color4b(255, 0, 0, 96));
                }
            }

            foreach (Particle particle in simulation.Particles)
            {
                Color4b color = ReferenceEquals(particle, selected) ? Color4b.Red : selectedNeighbors.Contains(particle) ? Color4b.Blue : Color4b.LightBlue;
                primitiveBatcher.AddCircle(particle.Position, particle.Radius, color);
            }

            uint minStorage = (uint)Math.Max(primitiveBatcher.TriangleVertexCount, primitiveBatcher.LineVertexCount);
            if (vertexBuffer.StorageLength < minStorage)
            {
                vertexBuffer.RecreateStorage((uint)Math.Max(primitiveBatcher.TriangleVertexCapacity, primitiveBatcher.LineVertexCapacity));
            }

            Vector2 simBorderSize = new Vector2(0.25f);
            Color4b borderColor = new Color4b(64, 64, 64);
            primitiveBatcher.AddRectangle(-simBorderSize, new Vector2(simSize.X + simBorderSize.X, 0), borderColor);
            primitiveBatcher.AddRectangle(new Vector2(-simBorderSize.X, simSize.Y), new Vector2(simSize.X + simBorderSize.X, simSize.Y + simBorderSize.Y), borderColor);
            primitiveBatcher.AddRectangle(new Vector2(-simBorderSize.X, 0), new Vector2(0, simSize.Y), borderColor);
            primitiveBatcher.AddRectangle(new Vector2(simSize.X, 0), new Vector2(simSize.X + simBorderSize.X, simSize.Y), borderColor);

            shaderProgram.View = Matrix4x4.CreateTranslation(5, 5, 0) * Matrix4x4.CreateScale(40);

            Color4b gridColor = new Color4b(96, 96, 96);
            for (float x = 0; x < simSize.X; x += binSize.X)
            {
                primitiveBatcher.AddLine(new Vector2(x, 0), new Vector2(x, simSize.Y), gridColor);
            }

            for (float y = 0; y < simSize.Y; y += binSize.Y)
            {
                primitiveBatcher.AddLine(new Vector2(0, y), new Vector2(simSize.X, y), gridColor);
            }

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;

            vertexBuffer.DataSubset.SetData(primitiveBatcher.LineVertices);
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, (uint)primitiveBatcher.LineVertexCount);

            vertexBuffer.DataSubset.SetData(primitiveBatcher.TriangleVertices);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)primitiveBatcher.TriangleVertexCount);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
            shaderProgram.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, 0, 1);
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int n)
        {
            if (key == Key.Space)
            {
                resetSimulation();
            }
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }
    }
}