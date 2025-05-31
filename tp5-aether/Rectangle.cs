using System.Numerics;

namespace tp5;

public struct Rectangle
{
    public Vector2 BottomLeft { get; set; }
    public Vector2 TopRight { get; set; }

    public Rectangle(Vector2 bottomLeft, Vector2 topRight)
    {
        BottomLeft = bottomLeft;
        TopRight = topRight;
    }

    public Rectangle((float, float) bottomLeft, (float, float) topRight)
    {
        BottomLeft = new Vector2(bottomLeft.Item1, bottomLeft.Item2);
        TopRight = new Vector2(topRight.Item1, topRight.Item2);
    }

    public Rectangle(float minX, float minY, float maxX, float maxY)
    {
        BottomLeft = new Vector2(minX, minY);
        TopRight = new Vector2(maxX, maxY);
    }

    public Vector2 TopLeft => new Vector2(BottomLeft.X, TopRight.Y);
    public Vector2 BottomRight => new Vector2(TopRight.X, BottomLeft.Y);
    public Vector2 Center => (BottomLeft + TopRight) / 2;

    public float Top => TopRight.Y;
    public float Bottom => BottomLeft.Y;
    public float Left => BottomLeft.X;
    public float Right => TopRight.X;

    public float Width => Right - Left;
    public float Height => Top - Bottom;
}