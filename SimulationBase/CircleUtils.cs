using System;
using System.Collections.Generic;
using System.Numerics;
using TrippyGL;

namespace SimulationBase;

public static class CircleUtils
{
    public static readonly Vector2[] circle = generateCircle(4);
    public static readonly Vector2[] circle2 = generateCircle(16);

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
        for (var i = 0; i < circle.Length - 2; i++)
        {
            primitiveBatcher.AddTriangle(
                new VertexColor(new Vector3(center + circle[i] * radius, depth), color),
                new VertexColor(new Vector3(center + circle[i + 1] * radius, depth), color),
                new VertexColor(new Vector3(center + circle[i + 2] * radius, depth), color)
            );
        }
    }

    public static void AddCirclePrecise(this PrimitiveBatcher<VertexColor> primitiveBatcher, Vector2 center, float radius, Color4b color, float depth = 0)
    {
        for (var i = 0; i < circle2.Length - 2; i++)
        {
            primitiveBatcher.AddTriangle(
                new VertexColor(new Vector3(center + circle2[i] * radius, depth), color),
                new VertexColor(new Vector3(center + circle2[i + 1] * radius, depth), color),
                new VertexColor(new Vector3(center + circle2[i + 2] * radius, depth), color)
            );
        }
    }
}