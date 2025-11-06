using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities.Models;

public class Line
{
    [JsonPropertyName("start")]
    public Point Start { get; set; }
    
    [JsonPropertyName("end")]
    public Point End { get; set; }
    
    [JsonPropertyName("dashed")]
    public bool Dashed { get; set; }

    public Line(Point startPoint, Point endPoint)
    {
        Start = startPoint;
        End = endPoint;
        Dashed = false;
    }

    // Required for JSON deserialization
    public Line()
    {
        Start = new Point();
        End = new Point();
    }

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
