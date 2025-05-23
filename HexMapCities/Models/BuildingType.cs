namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingType
{
    // base
    public string Name = string.Empty; // the name of this building type
    public List<string> MapImages = new(); // representation of this building on map
    public string UIImage = string.Empty;  // building UI image
    public string Description = string.Empty; // short description for what this building is used
    public int Type; // the type of this unit (value of an enum?)
    public int Era; // min era for this building
    public int Invention; // invention needed to build this building
    // economy
    public List<Tuple<int, int>> Goods_Production = new(); // goods produced by this building (goods id, amount)
    public List<Tuple<int, int>> Goods_Cost = new(); // goods needed to produce this building (goods id, amount)
    public int Food; // food produced by this building
    public int Production; // production of this building
    public int Gold; // gold produced by this building
    public int Science; // science produced by this building
    public int Citizens; // number of citizens this building habitats
    public int ProductionCost; // amount of production needed to build this building
    public int PurchaseCost; // amount of gold needed to purchase this building
}