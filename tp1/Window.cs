using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.Maths;
using SimulationBase;
using tp1.Simulate;
using TrippyGL;
using TrippyGL.Utils;

namespace tp1
{
    class Window : WindowBase
    {
        private Simulation simulation;

        private PrimitiveBatcher<VertexColor> primitiveBatcher;
        private VertexBuffer<VertexColor> vertexBuffer;
        private SimpleShaderProgram shaderProgram;

        public Window(Simulation simulation)
        {
            this.simulation = simulation;
        }

        protected override void OnLoad()
        {
            primitiveBatcher = new PrimitiveBatcher<VertexColor>(65536, 8192);

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)primitiveBatcher.TriangleVertexCapacity, BufferUsage.DynamicDraw);
            shaderProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            graphicsDevice.ClearColor = Color4b.White;
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
        }

        protected override void OnRender(double dt)
        {
            simulation.Step();
            graphicsDevice.Clear(ClearBuffers.Color);

            primitiveBatcher.ClearLines();
            primitiveBatcher.ClearTriangles();

            Particle selected = simulation.Particles.First();
            HashSet<Particle> selectedNeighbors = simulation.NeighborsDictionary[selected];

            primitiveBatcher.AddCirclePrecise(selected.Position, selected.Radius + simulation.NeighborRadius, new Color4b(255, 192, 192, 255));

            foreach (Particle particle in simulation.Particles)
            {
                Color4b color = ReferenceEquals(particle, selected) ? Color4b.Red : selectedNeighbors.Contains(particle) ? Color4b.Blue : Color4b.Cyan;
                primitiveBatcher.AddCircle(particle.Position, particle.Radius, color);
            }

            uint minStorage = (uint)Math.Max(primitiveBatcher.TriangleVertexCount, primitiveBatcher.LineVertexCount);
            if (vertexBuffer.StorageLength < minStorage)
            {
                vertexBuffer.RecreateStorage((uint)Math.Max(primitiveBatcher.TriangleVertexCapacity, primitiveBatcher.LineVertexCapacity));
            }

            shaderProgram.View = Matrix4x4.CreateScale(10);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;
            vertexBuffer.DataSubset.SetData(primitiveBatcher.TriangleVertices);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)primitiveBatcher.TriangleVertexCount);

            vertexBuffer.DataSubset.SetData(primitiveBatcher.LineVertices);
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, (uint)primitiveBatcher.LineVertexCount);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
            shaderProgram.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, 0, 1);
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }
    }
}