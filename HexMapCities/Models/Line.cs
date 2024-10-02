namespace HexMapCities.Models;

public class Line
{
    public Line(Point startPoint, Point endPoint)
    {
        Start = startPoint;
        End = endPoint;
    }

    public Point Start { get; set; } = new();
    public Point End { get; set; } = new();

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Line line &&
               Start.Equals(line.Start) &&
               End.Equals(line.End);
    }
}
