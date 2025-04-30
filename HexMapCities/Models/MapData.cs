namespace com.hexagonsimulations.HexMapCities.Models;

internal record MapData
{
    internal List<int> Map { get; set; } = new();
    internal int Rows { get; set; }
    internal int Columns { get; set; }
}
