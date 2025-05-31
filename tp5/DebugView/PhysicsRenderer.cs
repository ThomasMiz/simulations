using System.Numerics;
using TrippyGL;

namespace tp5.DebugView
{
    public class PhysicsRenderer : IDisposable
    {
        private const int CircleSegments = 32;
        private ComplexF circleSegmentRotation = ComplexF.FromAngle((float)(Math.PI * 2.0 / CircleSegments));

        private PrimitiveBatcher<VertexColor> batcher;

        private VertexBuffer<VertexColor> linesBuffer;
        private VertexBuffer<VertexColor> trianglesBuffer;

        private SimpleShaderProgram program;

        public PhysicsRenderer(GraphicsDevice graphicsDevice)
        {
            batcher = new PrimitiveBatcher<VertexColor>();

            program = SimpleShaderProgram.Create<VertexColor>(graphicsDevice, 0, 0, true);

            linesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)batcher.LineVertexCapacity, BufferUsage.StreamDraw);
            trianglesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)batcher.TriangleVertexCapacity, BufferUsage.StreamDraw);
        }

        public void DrawPoint(Vector2 p, float size, Color4b color)
        {
            Span<Vector2> verts = stackalloc Vector2[4];
            float hs = size / 2.0f;
            verts[0] = p + new Vector2(-hs, -hs);
            verts[1] = p + new Vector2(hs, -hs);
            verts[2] = p + new Vector2(hs, hs);
            verts[3] = p + new Vector2(-hs, hs);

            DrawSolidPolygon(verts, color, true);
        }

        /// <summary>
        /// Draw a closed polygon provided in CCW order.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="count">The vertex count.</param>
        /// <param name="color">The color value.</param>
        public void DrawPolygon(ReadOnlySpan<Vector2> vertices, Color4b color, bool closed = true)
        {
            for (int i = 0; i < vertices.Length - 1; i++)
                batcher.AddLine(new VertexColor(new Vector3(vertices[i], 0), color), new VertexColor(new Vector3(vertices[i + 1], 0), color));

            if (closed)
                batcher.AddLine(new VertexColor(new Vector3(vertices[^1], 0), color), new VertexColor(new Vector3(vertices[0], 0), color));
        }

        /// <summary>
        /// Draw a solid closed polygon provided in CCW order.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="count">The vertex count.</param>
        /// <param name="color">The color value.</param>
        public void DrawSolidPolygon(ReadOnlySpan<Vector2> vertices, Color4b color, bool outline = true)
        {
            if (vertices.Length == 2)
            {
                DrawPolygon(vertices, color);
                return;
            }

            Color4b colorFill = color * (outline ? 0.5f : 1.0f);

            for (int i = 1; i < vertices.Length - 1; i++)
            {
                batcher.AddTriangle(
                    new VertexColor(new Vector3(vertices[0], 0), colorFill),
                    new VertexColor(new Vector3(vertices[i], 0), colorFill),
                    new VertexColor(new Vector3(vertices[i + 1], 0), colorFill)
                );
            }

            if (outline)
                DrawPolygon(vertices, color);
        }

        /// <summary>
        /// Draw a circle.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="color">The color value.</param>
        public void DrawCircle(Vector2 center, float radius, Color4b color)
        {
            Vector2 v2 = new(radius, 0);
            var center_v2 = center + v2;
            var center_vS = center_v2;

            for (int i = 0; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = v2;
                var center_v1 = center_v2;
                ComplexF.Multiply(ref v1, ref circleSegmentRotation, out v2);
                center_v2 = Vector2.Add(center, v2);

                batcher.AddLine(new VertexColor(new Vector3(center_v1, 0), color), new VertexColor(new Vector3(center_v2, 0), color));
            }

            // Close Circle
            batcher.AddLine(new VertexColor(new Vector3(center_v2, 0), color), new VertexColor(new Vector3(center_vS, 0), color));
        }

        /// <summary>
        /// Draw a solid circle.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="color">The color value.</param>
        public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color4b color)
        {
            Vector2 v2 = new(radius, 0);
            var center_v2 = center + v2;
            var center_vS = center_v2;

            Color4b colorFill = color * 0.5f;

            for (int i = 0; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = v2;
                var center_v1 = center_v2;
                ComplexF.Multiply(ref v1, ref circleSegmentRotation, out v2);
                center_v2 = Vector2.Add(center, v2);

                // Draw Circle
                batcher.AddLine(new VertexColor(new Vector3(center_v1, 0), color), new VertexColor(new Vector3(center_v2, 0), color));

                // Draw Solid Circle
                if (i > 0)
                {
                    batcher.AddTriangle(
                        new VertexColor(new Vector3(center_vS, 0), colorFill),
                        new VertexColor(new Vector3(center_v1, 0), colorFill),
                        new VertexColor(new Vector3(center_v2, 0), colorFill)
                    );
                }
            }

            // Close Circle
            batcher.AddLine(new VertexColor(new Vector3(center_v2, 0), color), new VertexColor(new Vector3(center_vS, 0), color));

            DrawSegment(center, center + axis * radius, color);
        }

        /// <summary>
        /// Draw a line segment.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="color">The color value.</param>
        public void DrawSegment(Vector2 start, Vector2 end, Color4b color)
        {
            batcher.AddLine(new VertexColor(new Vector3(start, 0), color), new VertexColor(new Vector3(end, 0), color));
        }

        public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color4b color)
        {
            // Draw connection segment between start- and end-point
            DrawSegment(start, end, color);

            // Precalculate halfwidth
            float halfWidth = width / 2;

            // Create directional reference
            Vector2 rotation = Vector2.Normalize(start - end);

            // Calculate angle of directional vector
            float angle = (float)Math.Atan2(rotation.X, -rotation.Y);
            // Create matrix for rotation
            Matrix4x4 rotMatrix = Matrix4x4.CreateRotationZ(angle);
            // Create translation matrix for end-point
            Matrix4x4 endMatrix = Matrix4x4.CreateTranslation(end.X, end.Y, 0);

            // Setup arrow end shape
            Span<Vector2> verts = stackalloc Vector2[3];
            verts[0] = new Vector2(0, 0);
            verts[1] = new Vector2(-halfWidth, -length);
            verts[2] = new Vector2(halfWidth, -length);

            for (int i = 0; i < verts.Length; i++)
            {
                // Rotate end shape
                verts[i] = Vector2.Transform(verts[i], rotMatrix);
                // Translate end shape
                verts[i] = Vector2.Transform(verts[i], endMatrix);
            }

            // Draw arrow end shape
            DrawSolidPolygon(verts, color, false);

            if (drawStartIndicator)
            {
                // Create translation matrix for start
                Matrix4x4 startMatrix = Matrix4x4.CreateTranslation(start.X, start.Y, 0);
                // Setup arrow start shape
                Span<Vector2> baseVerts = stackalloc Vector2[4];
                baseVerts[0] = new Vector2(-halfWidth, length / 4);
                baseVerts[1] = new Vector2(halfWidth, length / 4);
                baseVerts[2] = new Vector2(halfWidth, 0);
                baseVerts[3] = new Vector2(-halfWidth, 0);

                for (int i = 0; i < baseVerts.Length; i++)
                {
                    // Rotate start shape
                    baseVerts[i] = Vector2.Transform(baseVerts[i], rotMatrix);
                    // Translate start shape
                    baseVerts[i] = Vector2.Transform(baseVerts[i], startMatrix);
                    // Draw start shape
                }

                DrawSolidPolygon(baseVerts, color, false);
            }
        }

        public void Begin()
        {
            batcher.ClearLines();
            batcher.ClearTriangles();
        }

        public void FlushAll(in Matrix4x4 view, in Matrix4x4 projection, BlendState blendState)
        {
            GraphicsDevice graphicsDevice = program.GraphicsDevice;
            graphicsDevice.BlendState = blendState;

            graphicsDevice.ShaderProgram = program;
            program.View = view;
            program.Projection = projection;

            if (batcher.TriangleVertexCount >= 3)
            {
                if (trianglesBuffer.StorageLength < batcher.TriangleVertexCount)
                    trianglesBuffer.RecreateStorage((uint)batcher.TriangleVertexCapacity, BufferUsage.StreamDraw);
                trianglesBuffer.DataSubset.SetData(batcher.TriangleVertices);
                graphicsDevice.VertexArray = trianglesBuffer;
                graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)batcher.TriangleVertexCount);
            }

            if (batcher.LineVertexCount >= 2)
            {
                if (linesBuffer.StorageLength < batcher.LineVertexCount)
                    linesBuffer.RecreateStorage((uint)batcher.LineVertexCapacity, BufferUsage.StreamDraw);
                linesBuffer.DataSubset.SetData(batcher.LineVertices);
                graphicsDevice.VertexArray = linesBuffer;
                graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, (uint)batcher.LineVertexCount);
            }

            batcher.ClearLines();
            batcher.ClearTriangles();
        }

        public void Dispose()
        {
            linesBuffer.Dispose();
            trianglesBuffer.Dispose();
            program.Dispose();
            linesBuffer = default;
            trianglesBuffer = default;
        }
    }
}