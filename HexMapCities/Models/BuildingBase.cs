using System.Text.Json.Serialization;
using com.hexagonsimulations.HexMapBase.Models;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingBase : BuildingType
{
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true; // is this building active?

    [JsonPropertyName("position")]
    public CubeCoordinates Position { get; set; } // its position on the map

    // Internal constructor - only accessible within the same assembly
    // This allows BuildingFactory to create instances while preventing external code from doing so
    // JSON deserializer can still access it since it's in the same assembly
    [JsonConstructor]
    internal BuildingBase()
    {
    }
}