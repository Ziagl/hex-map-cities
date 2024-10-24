namespace HexMapCities.Models;

public class Point
{
    public Point()
    {
        X = 0.0f;
        Y = 0.0f;
    }

    public Point(float v1, float v2)
    {
        X = v1;
        Y = v2;
    }

    public float X { get; set; }
    public float Y { get; set; }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Point point &&
               X == point.X &&
               Y == point.Y;
    }

    public static bool operator ==(Point left, Point right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Point left, Point right)
    {
        return !(left == right);
    }
}
