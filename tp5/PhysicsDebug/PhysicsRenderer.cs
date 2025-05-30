using System.Diagnostics;
using System.Numerics;
using AetherPhysics;
using AetherPhysics.Collision;
using AetherPhysics.Collision.Shapes;
using AetherPhysics.Common;
using AetherPhysics.Dynamics;
using AetherPhysics.Dynamics.Joints;
using TrippyGL;

namespace tp5.PhysicsDebug
{
    public class PhysicsRenderer : IDisposable
    {
        private const int CircleSegments = 32;
        private ComplexF circleSegmentRotation = ComplexF.FromAngle((float)(Math.PI * 2.0 / CircleSegments));
        private readonly Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];

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

        public void DrawAABB(in AABB aabb, Color4b color)
        {
            Span<Vector2> verts = stackalloc Vector2[4];
            verts[0] = aabb.LowerBound;
            verts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
            verts[2] = aabb.UpperBound;
            verts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

            DrawPolygon(verts, color);
        }

        public void DrawJoint(Joint joint)
        {
            if (!joint.Enabled)
                return;

            Body b1 = joint.BodyA;
            Body b2 = joint.BodyB;
            Transform xf1 = b1.GetTransform();

            Vector2 x2 = Vector2.Zero;

            // WIP David
            if (!joint.IsFixedType())
            {
                Transform xf2 = b2.GetTransform();
                x2 = xf2.p;
            }

            Vector2 p1 = joint.WorldAnchorA;
            Vector2 p2 = joint.WorldAnchorB;
            Vector2 x1 = xf1.p;

            Color4b color = new(0.5f, 0.8f, 0.8f);

            switch (joint.JointType)
            {
                case JointType.Distance:
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.Pulley:
                    PulleyJoint pulley = (PulleyJoint)joint;
                    Vector2 s1 = b1.GetWorldPoint(pulley.LocalAnchorA);
                    Vector2 s2 = b2.GetWorldPoint(pulley.LocalAnchorB);
                    DrawSegment(p1, p2, color);
                    DrawSegment(p1, s1, color);
                    DrawSegment(p2, s2, color);
                    break;
                case JointType.FixedMouse:
                    DrawPoint(p1, 0.5f, new Color4b(0.0f, 1.0f, 0.0f));
                    DrawSegment(p1, p2, new Color4b(0.8f, 0.8f, 0.8f));
                    break;
                case JointType.Revolute:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    DrawSegment(x2, p2, color);

                    DrawSolidCircle(p2, 0.1f, Vector2.Zero, Color4b.Red);
                    DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color4b.Blue);
                    break;
                case JointType.FixedAngle:
                    //Should not draw anything.
                    break;
                case JointType.FixedRevolute:
                    DrawSegment(x1, p1, color);
                    DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color4b.Pink);
                    break;
                case JointType.FixedLine:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.FixedDistance:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.FixedPrismatic:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.Gear:
                    DrawSegment(x1, x2, color);
                    break;
                default:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    DrawSegment(x2, p2, color);
                    break;
            }
        }

        /// <summary>
        /// Draw a transform. Choose your own length scale.
        /// </summary>
        /// <param name="transform">The transform.</param>
        public void DrawTransform(in Transform transform)
        {
            const float axisScale = 0.4f;
            Vector2 p1 = transform.p;

            var xAxis = transform.q.ToVector2();
            Vector2 p2 = p1 + axisScale * xAxis;
            DrawSegment(p1, p2, Color4b.Red);

            var yAxis = new Vector2(-transform.q.i, transform.q.R);
            p2 = p1 + axisScale * yAxis;
            DrawSegment(p1, p2, Color4b.Green);
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

        public void DrawShape(Fixture fixture, Transform xf, Color4b color)
        {
            if (fixture.IsSensor)
            {
                color.A = (byte)(color.A / 4);
            }
            
            switch (fixture.Shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.Shape;

                        Vector2 center = Transform.Multiply(circle.Position, ref xf);
                        float radius = circle.Radius;
                        Vector2 axis = xf.q.ToVector2();

                        DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.Shape;
                        int vertexCount = poly.Vertices.Count;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            _tempVertices[i] = Transform.Multiply(poly.Vertices[i], ref xf);
                        }

                        DrawSolidPolygon(_tempVertices.AsSpan(0, vertexCount), color);
                    }
                    break;


                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)fixture.Shape;
                        Vector2 v1 = Transform.Multiply(edge.Vertex1, ref xf);
                        Vector2 v2 = Transform.Multiply(edge.Vertex2, ref xf);
                        DrawSegment(v1, v2, color);
                    }
                    break;

                case ShapeType.Chain:
                    {
                        ChainShape chain = (ChainShape)fixture.Shape;

                        for (int i = 0; i < chain.Vertices.Count - 1; ++i)
                        {
                            Vector2 v1 = Transform.Multiply(chain.Vertices[i], ref xf);
                            Vector2 v2 = Transform.Multiply(chain.Vertices[i + 1], ref xf);
                            DrawSegment(v1, v2, color);
                        }
                    }
                    break;
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