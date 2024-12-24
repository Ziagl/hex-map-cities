using com.hexagonsimulations.HexMapBase.Geometry.Hex;

namespace com.hexagonsimulations.HexMapCities.Models;

public class CityBase
{
    // base
    public int Id;
    public int Player;
    public string Name = string.Empty;
    // position
    public CubeCoordinates Position;
    public List<CubeCoordinates> Tiles = new();
    // UI
    public Point PositionPixel = new();      // absolute position in pixels
    public List<Point> TilesPixel = new();   // absolute positions in pixels
    public List<Line> Borders = new();       // border lines in absolute pixels
    // buildings
    public List<BuildingBase> Buildings = new();
    // properties
    public Dictionary<string, object> Properties = new();
}
