using com.hexagonsimulations.HexMapBase.Models;
using com.hexagonsimulations.HexMapCities.Models;

namespace com.hexagonsimulations.HexMapCities.Tests.Models;

internal class TestUtils
{
    internal static CityBase CreateExampleCity1()
    {
        return new CityBase
        {
            Id = 0,
            Player = 1,
            Name = "City1",
            Position = new CubeCoordinates(0, 0, 0),
            Tiles = new List<CubeCoordinates>() { new CubeCoordinates(1, 0, -1), new CubeCoordinates(0, 1, -1) },
            PositionPixel = new(0, 0),
            TilesPixel = new() { new Point(34, 0), new Point(17, 24) },
            Borders = new(),
        };
    }

    internal static CityBase CreateExampleCity2()
    {
        return new CityBase
        {
            Id = 1,
            Player = 2,
            Name = "City2",
            Position = new CubeCoordinates(3, 0, -3),
            Tiles = new List<CubeCoordinates>() { new CubeCoordinates(2, 0, -2), new CubeCoordinates(2, 1, -3) },
            PositionPixel = new(102, 0),
            TilesPixel = new() { new Point(68, 0), new Point(85, 24) },
            Borders = new(),
        };
    }

    internal static BuildingBase CreateExampleBuilding(CubeCoordinates coordinates)
    {
        return new BuildingBase
        {
            Type = 1,
            Position = coordinates,
            Citizens = 2
        };
    }

    internal static CityBase CloneCity(CityBase city)
    {
        return new CityBase
        {
            Id = city.Id,
            Player = city.Player,
            Name = city.Name,
            Position = city.Position,
            Tiles = city.Tiles,
            PositionPixel = city.PositionPixel,
            TilesPixel = city.TilesPixel,
            Borders = city.Borders,
        };
    }

    internal static List<BuildingType> CreateBuildingTypes()
    {
        return new List<BuildingType>
        {
            new BuildingType(){
                Name = "Palace",
                Type = 1,
                Era = 1,
                Invention = 4,
                ProductionCost = 150,
                PurchaseCost = 500
            },
            new BuildingType(){
                Name = "Lumberjack",
                Type = 2,
                Era = 1,
                Invention = 4,
                ProductionCost = 150,
                PurchaseCost = 500
            }
        };
    }
}
