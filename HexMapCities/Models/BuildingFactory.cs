namespace com.hexagonsimulations.HexMapCities.Models;
internal class BuildingFactory
{
    private List<BuildingType> _buildingDefinitions = new();

    internal BuildingFactory(List<BuildingType> buildingDefinitions)
    {
        _buildingDefinitions = buildingDefinitions;
    }

    public BuildingBase? CreateBuilding(int buildingTypeId)
    {
        var definition = _buildingDefinitions.Find((definition) => definition.Type == buildingTypeId);
        if (definition is not null)
        {
            var building = new BuildingBase();
            building.Name = definition.Name;
            building.Description = definition.Description;
            building.Type = definition.Type;
            building.Era = definition.Era;
            building.Invention = definition.Invention;
            building.Production = definition.Production;
            building.Goods_Production = definition.Goods_Production;
            building.Goods_Cost = definition.Goods_Cost;
            building.Food = definition.Food;
            building.Production = definition.Production;
            building.Gold = definition.Gold;
            building.Science = definition.Science;
            building.ProductionCost = definition.ProductionCost;
            building.PurchaseCost = definition.PurchaseCost;
            return building;
        }
        else
        {
            return null;
        }
    }
}