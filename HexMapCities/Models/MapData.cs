namespace HexMapCities.Models;

public record MapData
{
    public List<int> Map { get; set; } = new();
    public int Rows { get; set; }
    public int Columns { get; set; }
}
