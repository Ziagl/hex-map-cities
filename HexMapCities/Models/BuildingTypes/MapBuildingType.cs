using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities.Models;

/*
 * adds map specific properties to BaseBuildingType
 * with them you can limit if the building can be built on specific tile characteristics
 */
public record MapBuildingType : BaseBuildingType
{
    [JsonPropertyName("landscapeType")]
    public List<int> LandscapeTypes { get; set; } = new();   // possiblelandscape types required to build this building (empty = any)

    [JsonPropertyName("terrainType")]
    public List<int> TerrainTypes { get; set; } = new();    // possible terrain types required to build this building (empty = any)

    [JsonPropertyName("minFood")]
    public int MinFood { get; set; }   // minimal food needed to build this building

    [JsonPropertyName("minProduction")]
    public int MinProduction { get; set; }   // minimal production needed to build this building
}