using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingType
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mapImages")]
    public List<string> MapImages { get; set; } = new();

    [JsonPropertyName("uiImage")]
    public string UIImage { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("era")]
    public int Era { get; set; }

    [JsonPropertyName("invention")]
    public int Invention { get; set; }

    [JsonPropertyName("foodLevel")]
    public int FoodLevel { get; set; }

    [JsonPropertyName("productionLevel")]
    public int ProductionLevel { get; set; }

    [JsonPropertyName("food")]
    public int Food { get; set; }

    [JsonPropertyName("production")]
    public int Production { get; set; }

    [JsonPropertyName("gold")]
    public int Gold { get; set; }

    [JsonPropertyName("science")]
    public int Science { get; set; }

    [JsonPropertyName("citizens")]
    public int Citizens { get; set; }

    [JsonPropertyName("goodsCost")]
    public Dictionary<int, int> GoodsCost { get; set; } = new();

    [JsonPropertyName("productionCost")]
    public int ProductionCost { get; set; }

    [JsonPropertyName("purchaseCost")]
    public int PurchaseCost { get; set; }

    [JsonPropertyName("upkeepCost")]
    public int UpkeepCost { get; set; }
}