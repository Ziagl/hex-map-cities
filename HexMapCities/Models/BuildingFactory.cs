namespace com.hexagonsimulations.HexMapCities.Models;
public static class BuildingFactory
{
    public static BuildingBase? CreateBuilding(BuildingType definition)
    {
        if (definition is not null)
        {
            var building = new BuildingBase();
            building.Name = definition.Name;
            building.MapImages = definition.MapImages;
            building.UIImage = definition.UIImage;
            building.Description = definition.Description;
            building.Type = definition.Type;
            building.Era = definition.Era;
            building.Invention = definition.Invention;
            building.FoodLevel = definition.FoodLevel;
            building.ProductionLevel = definition.ProductionLevel;
            building.Production = definition.Production;
            building.Food = definition.Food;
            building.Production = definition.Production;
            building.Gold = definition.Gold;
            building.Science = definition.Science;
            building.Citizens = definition.Citizens;
            building.GoodsCost = definition.GoodsCost;
            building.ProductionCost = definition.ProductionCost;
            building.PurchaseCost = definition.PurchaseCost;
            building.UpkeepCost = definition.UpkeepCost;
            return building;
        }
        else
        {
            return null;
        }
    }
}