namespace com.hexagonsimulations.HexMapCities.Models;
public static class BuildingFactory
{
    public static BuildingBase CreateBuilding(BuildingType definition)
        => new BuildingBase
        {
            Name = definition.Name,
            MapImages = definition.MapImages,
            UIImage = definition.UIImage,
            Description = definition.Description,
            Type = definition.Type,
            Invention = definition.Invention,
            Era = definition.Era,
            FoodLevel = definition.FoodLevel,
            ProductionLevel = definition.ProductionLevel,
            Production = definition.Production,
            Food = definition.Food,
            Gold = definition.Gold,
            Science = definition.Science,
            Citizens = definition.Citizens,
            GoodsCost = definition.GoodsCost,
            ProductionCost = definition.ProductionCost,
            PurchaseCost = definition.PurchaseCost,
            UpkeepCost = definition.UpkeepCost,
        };
}