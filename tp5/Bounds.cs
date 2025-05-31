using System.Numerics;
using Silk.NET.Maths;

namespace tp5;

public struct Bounds
{
    public Vector2D<double> BottomLeft { get; set; }
    public Vector2D<double> TopRight { get; set; }

    public Bounds(Vector2D<double> bottomLeft, Vector2D<double> topRight)
    {
        BottomLeft = bottomLeft;
        TopRight = topRight;
    }

    public Bounds((double, double) bottomLeft, (double, double) topRight)
    {
        BottomLeft = new Vector2D<double>(bottomLeft.Item1, bottomLeft.Item2);
        TopRight = new Vector2D<double>(topRight.Item1, topRight.Item2);
    }

    public Bounds(double minX, double minY, double maxX, double maxY)
    {
        BottomLeft = new Vector2D<double>(minX, minY);
        TopRight = new Vector2D<double>(maxX, maxY);
    }

    public Vector2D<double> TopLeft => new Vector2D<double>(BottomLeft.X, TopRight.Y);
    public Vector2D<double> BottomRight => new Vector2D<double>(TopRight.X, BottomLeft.Y);
    public Vector2D<double> Center => (BottomLeft + TopRight) / 2;

    public double Top => TopRight.Y;
    public double Bottom => BottomLeft.Y;
    public double Left => BottomLeft.X;
    public double Right => TopRight.X;

    public double Width => Right - Left;
    public double Height => Top - Bottom;
}