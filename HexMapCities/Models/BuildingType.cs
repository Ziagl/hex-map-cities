using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingType
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;    // name of the building

    [JsonPropertyName("mapImages")]
    public List<string> MapImages { get; set; } = new();    // list of images to display on the map (usually one, but more if there are other types - forest / jungle)
    
    [JsonPropertyName("models3d")]
    public List<string> Models3d { get; set; } = new();     // list of 3d models to display on the map (usually one, but more if there are other types - forest / jungle)

    [JsonPropertyName("uiImage")]
    public string UIImage { get; set; } = string.Empty;    // image to display 

    [JsonPropertyName("type")]
    public int Type { get; set; }   // type id of the building

    [JsonPropertyName("era")]
    public int Era { get; set; }    // minimal era required to build this building

    [JsonPropertyName("invention")]
    public int Invention { get; set; }  // invention needed to enable this building

    [JsonPropertyName("landscapeType")]
    public int LandscapeType { get; set; }  // landscape type required to build this building (0...none)

    [JsonPropertyName("terrainType")]
    public List<int> TerrainTypes { get; set; } = new();   // possible terrain types required to build this building (empty = any)

    [JsonPropertyName("minFood")]
    public int MinFood { get; set; }   // minimal food needed to build this building

    [JsonPropertyName("minProduction")]
    public int MinProduction { get; set; }   // minimal production needed to build this building

    [JsonPropertyName("citizens")]
    public int Citizens { get; set; }   // number of citizens can live in this building

    [JsonPropertyName("gold")]
    public int Gold { get; set; }   // gold produced per turn by this building

    [JsonPropertyName("science")]
    public int Science { get; set; }   // science produced per turn by this building

    [JsonPropertyName("goodsCost")]
    public Dictionary<int, int> GoodsCost { get; set; } = new();    // goods needed to build this building

    [JsonPropertyName("productionCost")]
    public int ProductionCost { get; set; } // gold needed to build this building

    [JsonPropertyName("purchaseCost")]
    public int PurchaseCost { get; set; }   // if set building can be bought for this amount of gold

    [JsonPropertyName("upkeepCost")]
    public int UpkeepCost { get; set; } // gold needed every turn to maintain this building
}