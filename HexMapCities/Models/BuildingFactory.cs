namespace com.hexagonsimulations.HexMapCities.Models;
public static class BuildingFactory
{
    public static BuildingBase CreateBuilding(BuildingType definition)
        => new BuildingBase
        {
            Name = definition.Name,
            MapImages = definition.MapImages,
            Models3d = definition.Models3d,
            UIImage = definition.UIImage,
            Type = definition.Type,
            Invention = definition.Invention,
            Era = definition.Era,
            LandscapeType = definition.LandscapeType,
            TerrainType = definition.TerrainType,
            Citizens = definition.Citizens,
            GoodsCost = definition.GoodsCost,
            ProductionCost = definition.ProductionCost,
            PurchaseCost = definition.PurchaseCost,
            UpkeepCost = definition.UpkeepCost,
        };
}