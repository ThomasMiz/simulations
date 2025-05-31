// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Numerics;
using System.Text;
using Silk.NET.Maths;
using tp5.Particles;
using TrippyGL;
using DRectangle = System.Drawing.Rectangle;

namespace tp5.DebugView
{
    public class DebugRenderer : IDisposable
    {
        public Simulation Simulation { get; }

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

        //Debug panel
        public Vector2 DebugPanelPosition = new(55, 100);
        private readonly StringBuilder debugPanelSbObjects = new();
        public bool ShowDebugPanel = true;

        // Grid
        public Vector2 GridMinimumCoords;
        public Vector2 GridMaximumCoords;
        public Vector2 GridStep = new(1, 1);
        public bool DrawGrid = true;

        public DebugRenderer(Simulation simulation, GraphicsDevice graphicsDevice, TextureFont font)
        {
            Simulation = simulation;
            device = graphicsDevice;

            batch = new TextureBatcher(device);
            batcherProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);
            batch.SetShaderProgram(batcherProgram);
            renderer = new PhysicsRenderer(graphicsDevice);
            this.font = font;
            stringData = new List<StringData>();
            BlendState = BlendState.NonPremultiplied;
        }

        #region IDisposable Members

        public void Dispose()
        {
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

        private void DrawDebugData()
        {
            if (DrawGrid)
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

            foreach (Particle p in Simulation.Particles)
            {
                renderer.DrawSolidCircle(p.Position.As<float>().ToSystem(), (float)p.Radius, new Vector2(1, 0), DefaultShapeColor);
            }

            if (ShowDebugPanel)
            {
                DrawDebugPanel();
            }
        }

        private void DrawDebugPanel()
        {
            debugPanelSbObjects.Clear();
            debugPanelSbObjects.Append("Steps: ").AppendNumber(Simulation.Steps);
            debugPanelSbObjects.Append("\nTime: ").AppendNumber(Simulation.SecondsElapsed).Append('s');
            debugPanelSbObjects.Append("\nParticles: ").AppendNumber(Simulation.Particles.Count);

            if (Simulation.HasStopped)
            {
                debugPanelSbObjects.Append("\nStopped. Press C+V to continue.");
            }

            DrawString(DebugPanelPosition, debugPanelSbObjects);
        }

        private void DrawString(Vector2 position, string text)
        {
            stringData.Add(new StringData(position, text, TextColor));
        }

        private void DrawString(Vector2 position, StringBuilder text)
        {
            stringData.Add(new StringData(position, text, TextColor));
        }

        public void RenderDebugData()
        {
            renderer.Begin();
            DrawDebugData();
            renderer.FlushAll(View, Projection, BlendState);

            Matrix4x4 localView = Matrix4x4.Identity;
            Matrix4x4 localProjection = Matrix4x4.CreateOrthographicOffCenter(0f, device.Viewport.Width, device.Viewport.Height, 0f, 0f, 1f);

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