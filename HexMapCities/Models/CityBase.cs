using com.hexagonsimulations.HexMapBase.Interfaces;
using com.hexagonsimulations.HexMapBase.Models;
using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities.Models;

public class CityBase : ICombatEntity
{
    // base entity
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("player")]
    public int Player { get; set; }
    
    [JsonPropertyName("health")]
    public int Health { get; set; }
    
    [JsonPropertyName("maxHealth")]
    public int MaxHealth { get; set; }
    
    // base city
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    // combat
    [JsonPropertyName("weaponType")]
    public int WeaponType { get; set; } // type of weapon/combat of this unit (infantry, cavalry, ...)

    [JsonPropertyName("combatStrength")]
    public int CombatStrength { get; set; } // attack points (damage in fight)

    [JsonPropertyName("rangedAttack")]
    public int RangedAttack { get; set; } // ranged attack points (damage of airstrike)

    [JsonPropertyName("range")]
    public int Range { get; set; } // attack range (how far can this unit attack)

    // random number
    [JsonPropertyName("seed")]
    public int Seed { get; set; } // random number seed (for random number generation)

    // position
    [JsonPropertyName("position")]
    public CubeCoordinates Position { get; set; }
    
    [JsonPropertyName("tiles")]
    public List<CubeCoordinates> Tiles { get; set; } = new();
    
    // UI
    [JsonPropertyName("positionPixel")]
    public Point PositionPixel { get; set; } = new(); // absolute position in pixels

    [JsonPropertyName("tilesPixel")]
    public List<Point> TilesPixel { get; set; } = new(); // absolute positions in pixels

    [JsonPropertyName("borders")]
    public List<Line> Borders { get; set; } = new(); // border lines in absolute pixels

    // buildings
    [JsonPropertyName("buildings")]
    public List<BuildingBase> Buildings { get; set; } = new();
    
    // inhabitants
    [JsonPropertyName("inhabitants")]
    public List<InhabitantBase> Inhabitants { get; set; } = new();
    
    // properties
    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; } = new();
}
