using System.Text.Json.Serialization;

using com.hexagonsimulations.HexMapBase.Models;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingBase : BuildingType
{
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true; // is this building active?

    [JsonPropertyName("position")]
    public CubeCoordinates Position { get; set; } // its position on the map
}