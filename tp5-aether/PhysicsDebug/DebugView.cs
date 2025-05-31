// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Numerics;
using System.Text;
using AetherPhysics.Collision;
using AetherPhysics.Collision.Shapes;
using AetherPhysics.Common;
using AetherPhysics.Controllers;
using AetherPhysics.Dynamics;
using AetherPhysics.Dynamics.Contacts;
using AetherPhysics.Dynamics.Joints;
using TrippyGL;

using DRectangle = System.Drawing.Rectangle;

namespace tp5.PhysicsDebug
{
    /// <summary>
    /// A debug view shows you what happens inside the physics engine. You can view
    /// bodies, joints, fixtures and more.
    /// </summary>
    public class DebugView : IDisposable
    {
        /// <summary>
        /// Whether this <see cref="DebugView"/> is enabled. If it's not, nothing will be rendered.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// The <see cref="Dynamics.World"/> this <see cref="DebugView"/> renders info about.
        /// </summary>
        public readonly World World;

        /// <summary>
        /// Gets or sets the debug view flags.
        /// </summary>
        public DebugViewFlags Flags { get; set; }

        //Drawing
        private PhysicsRenderer renderer;

        private TextureBatcher batch;
        private SimpleShaderProgram batcherProgram;
        public BlendState BlendState;
        public Matrix4x4 View = Matrix4x4.Identity, Projection = Matrix4x4.Identity;

        private TextureFont font;
        private GraphicsDevice device;
        private readonly List<StringData> stringData;

        //Shapes
        public readonly Color4b DefaultShapeColor = new(0.9f, 0.7f, 0.7f);
        public readonly Color4b InactiveShapeColor = new(0.5f, 0.5f, 0.3f);
        public readonly Color4b KinematicShapeColor = new(0.5f, 0.5f, 0.9f);
        public readonly Color4b SleepingShapeColor = new(0.6f, 0.6f, 0.6f);
        public readonly Color4b StaticShapeColor = new(0.5f, 0.9f, 0.5f);
        public readonly Color4b TextColor = Color4b.White;
        public readonly Color4b GridColor = new(0.35f, 0.35f, 0.35f, 0.4f);

        //Contacts
        private int pointCount;
        private const int MaxContactPoints = 2048;
        private readonly ContactPoint[] points = new ContactPoint[MaxContactPoints];

        //Debug panel
        public Vector2 DebugPanelPosition = new(55, 100);
        public int DebugPanelTextDiffX = 150;
        private TimeSpan min, max, avg;
        private readonly StringBuilder graphSbMax = new();
        private readonly StringBuilder graphSbAvg = new();
        private readonly StringBuilder graphSbMin = new();
        private readonly StringBuilder debugPanelSbObjects = new();
        private readonly StringBuilder debugPanelSbUpdate = new();

        //Performance graph
        public DRectangle PerformancePanelBounds = new(330, 100, 200, 100);
        public bool AdaptiveLimits = true;
        public int ValuesToGraph = 500;
        public TimeSpan MinimumValue;
        public TimeSpan MaximumValue = TimeSpan.FromMilliseconds(10);
        private bool updatePerformanceGraphCalled = false;
        private readonly List<TimeSpan> graphValues = new(500);
        private readonly Vector2[] background = new Vector2[4];

        // Grid
        public Vector2 GridMinimumCoords;
        public Vector2 GridMaximumCoords;
        public Vector2 GridStep = new(1, 1);

        public DebugView(World world, GraphicsDevice graphicsDevice, TextureFont font)
        {
            World = world;
            world.ContactManager.PreSolve += PreSolve;

            //Default flags
            AppendFlags(DebugViewFlags.Shape);
            AppendFlags(DebugViewFlags.Controllers);
            AppendFlags(DebugViewFlags.Joint);

            device = graphicsDevice;
            // Create a new SpriteBatch, which can be used to draw textures.
            batch = new TextureBatcher(device);
            batcherProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);
            batch.SetShaderProgram(batcherProgram);
            renderer = new PhysicsRenderer(graphicsDevice);
            this.font = font;
            stringData = new List<StringData>();
            BlendState = BlendState.NonPremultiplied;
        }

        /// <summary>
        /// Append flags to the current flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void AppendFlags(DebugViewFlags flags)
        {
            Flags |= flags;
        }

        /// <summary>
        /// Remove flags from the current flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public void RemoveFlags(DebugViewFlags flags)
        {
            Flags &= ~flags;
        }

        #region IDisposable Members

        public void Dispose()
        {
            World.ContactManager.PreSolve -= PreSolve;
            renderer.Dispose();
            batch.Dispose();
            batcherProgram.Dispose();

            font = null;
            device = null;
            renderer = null;
            batch = null;
            batcherProgram = null;
        }

        #endregion

        private void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                Manifold manifold = contact.Manifold;

                if (manifold.PointCount == 0)
                    return;

                Fixture fixtureA = contact.FixtureA;
                Collision.GetPointStates(out _, out FixedArray2<PointState> state2, ref oldManifold, ref manifold);

                contact.GetWorldManifold(out Vector2 normal, out FixedArray2<Vector2> points);

                for (int i = 0; i < manifold.PointCount && pointCount < MaxContactPoints; ++i)
                {
                    if (fixtureA == null)
                        this.points[i] = new ContactPoint();

                    ContactPoint cp = this.points[pointCount];
                    cp.Position = points[i];
                    cp.Normal = normal;
                    cp.State = state2[i];
                    this.points[pointCount] = cp;
                    pointCount++;
                }
            }
        }

        private Color4b GetColorByType(BodyType bodyType)
        {
            return bodyType switch
            {
                BodyType.Static => StaticShapeColor,
                BodyType.Kinematic => KinematicShapeColor,
                _ => DefaultShapeColor
            };
        }

        /// <summary>
        /// Call this to draw shapes and other debug draw data.
        /// </summary>
        private void DrawDebugData()
        {
            if ((Flags & DebugViewFlags.Grid) == DebugViewFlags.Grid)
            {
                for (float x = 0; x < GridMaximumCoords.X; x += GridStep.X)
                    renderer.DrawSegment(new Vector2(x, GridMinimumCoords.Y), new Vector2(x, GridMaximumCoords.Y), GridColor);
                for (float x = 0; x > GridMinimumCoords.X; x -= GridStep.X)
                    renderer.DrawSegment(new Vector2(x, GridMinimumCoords.Y), new Vector2(x, GridMaximumCoords.Y), GridColor);

                for (float y = 0; y < GridMaximumCoords.Y; y += GridStep.Y)
                    renderer.DrawSegment(new Vector2(GridMinimumCoords.X, y), new Vector2(GridMaximumCoords.X, y), GridColor);
                for (float y = 0; y > GridMinimumCoords.Y; y -= GridStep.Y)
                    renderer.DrawSegment(new Vector2(GridMinimumCoords.X, y), new Vector2(GridMaximumCoords.X, y), GridColor);
            }

            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                foreach (Body b in World.BodyList)
                    foreach (Fixture f in b.FixtureList)
                        renderer.DrawShape(f, b.GetTransform(), b.Enabled ? GetColorByType(b.BodyType) : SleepingShapeColor);
            }

            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                const float axisScale = 0.3f;

                for (int i = 0; i < pointCount; ++i)
                {
                    ContactPoint point = points[i];

                    if (point.State == PointState.Add)
                        renderer.DrawPoint(point.Position, 0.1f, new Color4b(76, 242, 76));
                    else if (point.State == PointState.Persist)
                        renderer.DrawPoint(point.Position, 0.1f, new Color4b(76, 76, 242));

                    if ((Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
                    {
                        Vector2 p1 = point.Position;
                        Vector2 p2 = p1 + axisScale * point.Normal;
                        renderer.DrawSegment(p1, p2, new Color4b(102, 229, 102));
                    }
                }

                pointCount = 0;
            }

            if ((Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
            {
                foreach (Body body in World.BodyList)
                    foreach (Fixture f in body.FixtureList)
                        if (f.Shape is PolygonShape polygon)
                        {
                            Transform xf = body.GetTransform();

                            for (int i = 0; i < polygon.Vertices.Count; i++)
                            {
                                Vector2 tmp = Transform.Multiply(polygon.Vertices[i], ref xf);
                                renderer.DrawPoint(tmp, 0.1f, Color4b.Red);
                            }
                        }
            }

            if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
            {
                foreach (Joint j in World.JointList)
                    renderer.DrawJoint(j);
            }

            if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
            {
                Color4b color = new(229, 76, 229);
                IBroadPhase bp = World.ContactManager.BroadPhase;

                foreach (Body body in World.BodyList)
                {
                    if (body.Enabled == false)
                        continue;

                    foreach (Fixture f in body.FixtureList)
                        for (int t = 0; t < f.ProxyCount; ++t)
                        {
                            FixtureProxy proxy = f.Proxies[t];
                            bp.GetFatAABB(proxy.ProxyId, out AABB aabb);

                            renderer.DrawAABB(aabb, color);
                        }
                }
            }

            if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf = b.GetTransform();
                    xf.p = b.WorldCenter;
                    renderer.DrawTransform(xf);
                }
            }

            if ((Flags & DebugViewFlags.Controllers) == DebugViewFlags.Controllers)
            {
                for (int i = 0; i < World.ControllerList.Count; i++)
                    if (World.ControllerList[i] is BuoyancyController buoyancy)
                        renderer.DrawAABB(buoyancy.Container, Color4b.LightBlue);
            }

            if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
                DrawDebugPanel();
        }

        private void DrawPerformanceGraph()
        {
            float x = PerformancePanelBounds.X;
            float deltaX = PerformancePanelBounds.Width / (float)ValuesToGraph;
            float yScale = PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;

            // we must have at least 2 values to start rendering
            if (graphValues.Count > 2)
            {
                min = TimeSpan.MaxValue;
                max = TimeSpan.Zero;
                avg = TimeSpan.Zero;
                for (int i = 0; i < graphValues.Count; i++)
                {
                    var val = graphValues[i];
                    min = TimeSpan.FromTicks(Math.Min(min.Ticks, val.Ticks));
                    max = TimeSpan.FromTicks(Math.Max(max.Ticks, val.Ticks));
                    avg += val;
                }
                avg = TimeSpan.FromTicks(avg.Ticks / graphValues.Count);

                if (AdaptiveLimits)
                {
                    var maxTicks = max.Ticks;
                    // Round up to the next highest power of 2
                    {
                        maxTicks--;
                        maxTicks |= maxTicks >> 1;
                        maxTicks |= maxTicks >> 2;
                        maxTicks |= maxTicks >> 4;
                        maxTicks |= maxTicks >> 8;
                        maxTicks |= maxTicks >> 16;
                        maxTicks++;
                    }
                    MaximumValue = TimeSpan.FromTicks(maxTicks);
                    MinimumValue = TimeSpan.Zero;
                }

                // start at last value (newest value added)
                // continue until no values are left
                for (int i = graphValues.Count - 1; i > 0; i--)
                {
                    float y1 = PerformancePanelBounds.Bottom - (((yScale * graphValues[i].Ticks) / (MaximumValue - MinimumValue).Ticks));
                    float y2 = PerformancePanelBounds.Bottom - (((yScale * graphValues[i - 1].Ticks) / (MaximumValue - MinimumValue).Ticks));

                    Vector2 x1 = new(Math.Clamp(x, PerformancePanelBounds.Left, PerformancePanelBounds.Right), Math.Clamp(y1, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
                    Vector2 x2 = new(Math.Clamp(x + deltaX, PerformancePanelBounds.Left, PerformancePanelBounds.Right), Math.Clamp(y2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    renderer.DrawSegment(x1, x2, Color4b.LightGreen);

                    x += deltaX;
                }
            }

            graphSbMax.Clear(); graphSbAvg.Clear(); graphSbMin.Clear();
            int centerY = (PerformancePanelBounds.Top + PerformancePanelBounds.Bottom) / 2;
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, graphSbMax.Append("Max: ").AppendNumber((float)max.TotalMilliseconds, 3).Append(" ms"));
            DrawString(PerformancePanelBounds.Right + 10, centerY - 7, graphSbAvg.Append("Avg: ").AppendNumber((float)avg.TotalMilliseconds, 3).Append(" ms"));
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, graphSbMin.Append("Min: ").AppendNumber((float)min.TotalMilliseconds, 3).Append(" ms"));

            //Draw background.
            background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
            background[1] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);

            renderer.DrawSolidPolygon(background, Color4b.DarkGray, true);
        }

        private void UpdatePerformanceGraph(TimeSpan updateTime)
        {
            graphValues.Add(updateTime);

            if (graphValues.Count > ValuesToGraph + 1)
                graphValues.RemoveAt(0);

            updatePerformanceGraphCalled = true;
        }

        private void DrawDebugPanel()
        {
            int fixtureCount = 0;
            for (int i = 0; i < World.BodyList.Count; i++)
                fixtureCount += World.BodyList[i].FixtureList.Count;

            int x = (int)DebugPanelPosition.X;
            int y = (int)DebugPanelPosition.Y;

            debugPanelSbObjects.Clear();
            debugPanelSbObjects.Append("Objects:\n");
            debugPanelSbObjects.Append("- Bodies:   \n").AppendNumber(World.BodyList.Count);
            debugPanelSbObjects.Append("- Fixtures: \n").AppendNumber(fixtureCount);
            debugPanelSbObjects.Append("- Contacts: \n").AppendNumber(World.ContactCount);
            debugPanelSbObjects.Append("- Proxies:  \n").AppendNumber(World.ProxyCount);
            debugPanelSbObjects.Append("- Joints:   \n").AppendNumber(World.JointList.Count);
            debugPanelSbObjects.Append("- Controllers: \n").AppendNumber(World.ControllerList.Count);
            DrawString(x, y, debugPanelSbObjects);

            debugPanelSbUpdate.Clear();
            debugPanelSbUpdate.Append("Update time:\n");
            debugPanelSbUpdate.Append("- Body:    \n").AppendNumber((float)World.SolveUpdateTime.TotalMilliseconds, 3).Append(" ms");
            debugPanelSbUpdate.Append("- Contact: \n").AppendNumber((float)World.ContactsUpdateTime.TotalMilliseconds, 3).Append(" ms");
            debugPanelSbUpdate.Append("- CCD:     \n").AppendNumber((float)World.ContinuousPhysicsTime.TotalMilliseconds, 3).Append(" ms");
            debugPanelSbUpdate.Append("- Joint:   \n").AppendNumber((float)World.Island.JointUpdateTime.TotalMilliseconds, 3).Append(" ms");
            debugPanelSbUpdate.Append("- Controller:\n").AppendNumber((float)World.ControllersUpdateTime.TotalMilliseconds, 3).Append(" ms");
            debugPanelSbUpdate.Append("- Total:   \n").AppendNumber((float)World.UpdateTime.TotalMilliseconds, 3).Append(" ms");
            DrawString(x + DebugPanelTextDiffX, y, debugPanelSbUpdate);
        }

        private void DrawString(Vector2 position, string text)
        {
            stringData.Add(new StringData(position, text, TextColor));
        }

        private void DrawString(int x, int y, StringBuilder text)
        {
            DrawString(new Vector2(x, y), text);
        }

        private void DrawString(Vector2 position, StringBuilder text)
        {
            stringData.Add(new StringData(position, text, TextColor));
        }

        public void RenderDebugData()
        {
            if (!Enabled)
                return;

            if (!updatePerformanceGraphCalled)
                UpdatePerformanceGraph(World.UpdateTime);
            updatePerformanceGraphCalled = false;

            //Nothing is enabled - don't draw the debug view.
            if (Flags == 0)
                return;

            renderer.Begin();
            DrawDebugData();
            renderer.FlushAll(View, Projection, BlendState);

            Matrix4x4 localView = Matrix4x4.Identity;
            Matrix4x4 localProjection = Matrix4x4.CreateOrthographicOffCenter(0f, device.Viewport.Width, device.Viewport.Height, 0f, 0f, 1f);

            if ((Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
            {
                renderer.Begin();
                DrawPerformanceGraph();
                renderer.FlushAll(localView, localProjection, BlendState);
            }

            // begin the sprite batch effect
            batch.Begin(BatcherBeginMode.OnTheFly);
            batcherProgram.View = localView;
            batcherProgram.Projection = localProjection;

            // draw any strings we have
            for (int i = 0; i < stringData.Count; i++)
            {
                if (stringData[i].Text != null)
                    batch.DrawString(font, stringData[i].Text, stringData[i].Position, stringData[i].Color);
                else
                    batch.DrawString(font, stringData[i].stringBuilderText, stringData[i].Position, stringData[i].Color);
            }

            // end the sprite batch effect
            batch.End();

            stringData.Clear();
        }

        #region Nested type: ContactPoint

        private struct ContactPoint
        {
            public Vector2 Normal;
            public Vector2 Position;
            public PointState State;
        }

        #endregion

        #region Nested type: StringData

        private struct StringData
        {
            public readonly Color4b Color;
            public readonly string Text;
            public readonly StringBuilder stringBuilderText;
            public readonly Vector2 Position;

            public StringData(Vector2 position, string text, Color4b color)
            {
                Position = position;
                Text = text;
                stringBuilderText = null;
                Color = color;
            }

            public StringData(Vector2 position, StringBuilder text, Color4b color)
            {
                Position = position;
                Text = null;
                stringBuilderText = text;
                Color = color;
            }
        }

        #endregion
    }
}