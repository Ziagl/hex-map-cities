using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities;

public class MapData
{
    [JsonPropertyName("map")]
    public List<int> Map { get; set; } = new();

    [JsonPropertyName("rows")]
    public int Rows { get; set; }

    [JsonPropertyName("columns")]
    public int Columns { get; set; }

    public MapData()
    {
    }

    public MapData Clone()
    {
        return new MapData
        {
            Map = new List<int>(Map),
            Rows = Rows,
            Columns = Columns
        };
    }
}
