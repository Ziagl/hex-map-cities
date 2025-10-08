namespace com.hexagonsimulations.HexMapCities.Models;

internal class MapData
{
    internal List<int> Map { get; set; } = new();
    internal int Rows { get; set; }
    internal int Columns { get; set; }

    public MapData() { }

    public MapData Clone()
    {
        return new MapData
        {
            Rows = this.Rows,
            Columns = this.Columns,
            Map = this.Map is null ? new List<int>() : new List<int>(this.Map)
        };
    }
}
