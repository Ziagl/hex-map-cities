namespace com.hexagonsimulations.HexMapCities.Models;

public class Line
{
    public Line(Point startPoint, Point endPoint)
    {
        Start = startPoint;
        End = endPoint;
    }

    public Point Start { get; set; } = new();
    public Point End { get; set; } = new();
    public bool Dashed { get; set; } = false;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Line line && 
               ((Start.Equals(line.Start) &&
               (End.Equals(line.End))) ||
               (Start.Equals(line.End) &&
               (End.Equals(line.Start))));
    }

    public static bool operator ==(Line left, Line right)
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

    public static bool operator !=(Line left, Line right)
    {
        return !(left == right);
    }
}
