using System;
using System.Collections.Generic;
using System.Numerics;
using TrippyGL;

namespace SimulationBase;

public static class PrimitiveBatcherExtensions
{
    public static readonly Vector2[] circleSmall = generateCircle(4);
    public static readonly Vector2[] circlePrecise = generateCircle(16);

    /// <summary>
    /// Generates a triangle strip with the shape of a circle with radius 1 centered at (0, 0).
    /// </summary>
    public static Vector2[] generateCircle(int divisions)
    {
        List<Vector2> strip = new();

        strip.Add(new Vector2(-1, 0));

        for (int i = divisions; i > 0; i--)
        {
            float x = MathF.Sqrt(i / (float)(divisions + 1));
            float y = MathF.Sqrt(1 - x * x);

            strip.Add(new Vector2(-x, -y));
            strip.Add(new Vector2(-x, y));
        }

        strip.Add(new Vector2(0, -1));
        strip.Add(new Vector2(0, 1));

        for (int i = 1; i <= divisions; i++)
        {
            float x = MathF.Sqrt(i / (float)(divisions + 1));
            float y = MathF.Sqrt(1 - x * x);

            strip.Add(new Vector2(x, -y));
            strip.Add(new Vector2(x, y));
        }

        strip.Add(new Vector2(1, 0));

        return strip.ToArray();
    }

    public static void AddCircle(this PrimitiveBatcher<VertexColor> primitiveBatcher, Vector2 center, float radius, Color4b color, float depth = 0)
    {
        for (var i = 0; i < circleSmall.Length - 2; i++)
        {
            primitiveBatcher.AddTriangle(
                new VertexColor(new Vector3(center + circleSmall[i] * radius, depth), color),
                new VertexColor(new Vector3(center + circleSmall[i + 1] * radius, depth), color),
                new VertexColor(new Vector3(center + circleSmall[i + 2] * radius, depth), color)
            );
        }
    }

    public static void AddCirclePrecise(this PrimitiveBatcher<VertexColor> primitiveBatcher, Vector2 center, float radius, Color4b color, float depth = 0)
    {
        for (var i = 0; i < circlePrecise.Length - 2; i++)
        {
            primitiveBatcher.AddTriangle(
                new VertexColor(new Vector3(center + circlePrecise[i] * radius, depth), color),
                new VertexColor(new Vector3(center + circlePrecise[i + 1] * radius, depth), color),
                new VertexColor(new Vector3(center + circlePrecise[i + 2] * radius, depth), color)
            );
        }
    }

    public static void AddRectangle(this PrimitiveBatcher<VertexColor> primitiveBatcher, Vector2 tl, Vector2 br, Color4b color, float depth = 0)
    {
        primitiveBatcher.AddTriangle(
            new VertexColor(new Vector3(new Vector2(tl.X, tl.Y), depth), color),
            new VertexColor(new Vector3(new Vector2(tl.X, br.Y), depth), color),
            new VertexColor(new Vector3(new Vector2(br.X, tl.Y), depth), color)
        );

        primitiveBatcher.AddTriangle(
            new VertexColor(new Vector3(new Vector2(tl.X, br.Y), depth), color),
            new VertexColor(new Vector3(new Vector2(br.X, tl.Y), depth), color),
            new VertexColor(new Vector3(new Vector2(br.X, br.Y), depth), color)
        );
    }

    public static void AddLine(this PrimitiveBatcher<VertexColor> primitiveBatcher, Vector2 from, Vector2 to, Color4b color, float depth = 0)
    {
        primitiveBatcher.AddLine(new VertexColor(new Vector3(from, depth), color), new VertexColor(new Vector3(to, depth), color));
    }
}