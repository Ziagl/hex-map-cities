using com.hexagonsimulations.HexMapBase.Models;
using com.hexagonsimulations.HexMapCities.Enums;
using com.hexagonsimulations.HexMapCities.Models;
using com.hexagonsimulations.HexMapCities.Tests.Models;
using System.ComponentModel.DataAnnotations;

namespace com.hexagonsimulations.HexMapCities.Tests;

[TestClass]
public sealed class CityManagerTests
{
    private readonly int _tileWidth = 34;
    private readonly int _tileHeight = 32;

    [TestMethod]
    public void CreateCity()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
    }

    [TestMethod]
    public void CreateCityCollision()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        exampleMap[0] = (int)TileType.UNBUILDABLE;
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>() { (int)TileType.UNBUILDABLE }, new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public void GetCityById()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city1 = cityManager.GetCityById(1);
        Assert.IsNotNull(city1);
        Assert.AreEqual("City1", city1.Name);
        var undefinedCity = cityManager.GetCityById(2);
        Assert.IsNull(undefinedCity);
    }

    [TestMethod]
    public void GetCityByCoordinates()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city1 = cityManager.GetCityById(1);
        Assert.IsNotNull(city1);
        Assert.AreEqual("City1", city1.Name);
        var undefinedCity = cityManager.GetCityByCoordinates(city.Position);
        Assert.IsNotNull(undefinedCity);
        undefinedCity = cityManager.GetCityByCoordinates(new CubeCoordinates(1, 0, 0));
        Assert.IsNull(undefinedCity);
    }

    [TestMethod]
    public void GetCitiesOfPlayer()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity2();
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        var city3 = CreateExampleCity1();
        city3.Name = "City3";
        city3.Position = new CubeCoordinates(0, 2, -2);
        city3.Player = 2;
        success = cityManager.CreateCity(city3);
        Assert.IsTrue(success);
        var cities = cityManager.GetCitiesOfPlayer(2);
        Assert.AreEqual(2, cities.Count);
        cities = cityManager.GetCitiesOfPlayer(1);
        Assert.AreEqual(1, cities.Count);
    }

    [TestMethod]
    public void CreateCityBorders()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(12, city.Borders.Count);
    }

    [TestMethod]
    public void CreateCityAddTilesAndBorders()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>(), _tileHeight, _tileWidth);
        var city = CreateExampleCity1();
        var tiles = city.Tiles;
        city.PositionPixel = new Point(16, 17);
        city.Tiles = new();
        city.TilesPixel = new();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        foreach (var tile in tiles)
        {
            cityManager.AddCityTile(city.Id, tile);
        }
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(12, city.Borders.Count);
    }

    [TestMethod]
    public void CreateCityBordersMoreCities()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity1();
        city2.Position = new CubeCoordinates(2, 1, -3);
        city2.Tiles = new List<CubeCoordinates>() { new CubeCoordinates(2, 0, -2), new CubeCoordinates(3, 0, -3), new CubeCoordinates(1, 1, -2), new CubeCoordinates(1, 2, -3), new CubeCoordinates(2, 2, -4) };
        city2.PositionPixel = new(85, 24);
        city2.TilesPixel = new() { new Point(68, 0), new Point(102, 0), new Point(51, 24), new Point(68, 48), new Point(102, 48) };
        city2.Borders = new();
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(9, city.Borders.Count);
        Assert.AreEqual(15, city2.Borders.Count);
    }

    [TestMethod]
    public void TwoCitiesTouchOnOneLine()
    {
        var exampleMap = Enumerable.Repeat(0, 36).ToList();
        var cityManager = new CityManager(exampleMap, 6, 6, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity1();
        city2.Position = new CubeCoordinates(0, 3, -3);
        city2.PositionPixel = new(51, 72);
        city2.Tiles = new();
        city2.TilesPixel = new();
        city2.Borders = new();
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        var tileList = new List<CubeCoordinates>() { new CubeCoordinates(0, 2, -2), new CubeCoordinates(1, 2, -3), new CubeCoordinates(1, 3, -4), new CubeCoordinates(0, 4, -4), new CubeCoordinates(-1, 4, -3), new CubeCoordinates(-1, 3, -2) };
        foreach (var tile in tileList)
        {
            cityManager.AddCityTile(city2.Id, tile);
        }
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(11, city.Borders.Count);
        Assert.AreEqual(17, city2.Borders.Count);
    }

    [TestMethod]
    public void AddCityTile()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        // add tile to which is already part of city
        var newTile = new CubeCoordinates(1, 0, -1);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsFalse(success);
        // add tile to which is not part of city
        newTile = new CubeCoordinates(2, 0, -2);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsTrue(success);
        Assert.AreEqual(3, city.Tiles.Count);
    }

    [TestMethod]
    public void AddCityTileNotValid()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        Assert.AreEqual(2, city.Tiles.Count);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        // add tile with a gap to city fails
        var newTile = new CubeCoordinates(3, 0, -3);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsFalse(success);
        // close the gap
        newTile = new CubeCoordinates(2, 0, -2);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsTrue(success);
        // now same tile can be added
        newTile = new CubeCoordinates(3, 0, -3);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsTrue(success);
        Assert.AreEqual(4, city.Tiles.Count);
    }

    [TestMethod]
    public void AddCityTileCollisionWithOtherCity()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity2();
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        // add tile to which is already part of city
        var newTile = new CubeCoordinates(2, 0, -2);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsFalse(success);
        // add tile to which is already part of other city
        newTile = new CubeCoordinates(2, 0, -2);
        success = cityManager.AddCityTile(city2.Id, newTile);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public void CreateCityBordersAfterTileWasAdded()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(12, city.Borders.Count);
        var newTile = new CubeCoordinates(1, 1, -2);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(14, city.Borders.Count);
    }

    [TestMethod]
    public void CreateCityBordersMoreCitiesAndAddTile()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity2();
        city2.Player = 1;
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city2.Player);
        Assert.AreEqual(22, city.Borders.Count + city2.Borders.Count);
        var newTile = new CubeCoordinates(1, 1, -2);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(20, city.Borders.Count + city2.Borders.Count);
    }

    [TestMethod]
    public void CreateCityBordersMoreCitiesDashedBorder()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity2();
        city2.Player = 2;
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        cityManager.CreateCityBorders(city2.Player);
        Assert.AreEqual(24, city.Borders.Count + city2.Borders.Count);
        var newTile = new CubeCoordinates(1, 1, -2);
        success = cityManager.AddCityTile(city.Id, newTile);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.IsTrue(city2.Borders[7].Dashed);
    }

    [TestMethod]
    public void CityProperties()
    {
        var city = CreateExampleCity1();
        city.Properties.Add("sprite.position", new Vector2() { X = 1.0f, Y = 2.0f });
        city.Properties.Add("world.position", new Vector3() { X = 3.0f, Y = 2.0f, Z = 1.0f });
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var spritePosition = city.Properties["sprite.position"];
        Assert.AreEqual(1.0f, ((Vector2)spritePosition).X);
        Assert.AreEqual(2.0f, ((Vector2)spritePosition).Y);
        var worldPosition = city.Properties["world.position"];
        Assert.AreEqual(3.0f, ((Vector3)worldPosition).X);
        Assert.AreEqual(2.0f, ((Vector3)worldPosition).Y);
        Assert.AreEqual(1.0f, ((Vector3)worldPosition).Z);
    }

    [TestMethod]
    public void IsTileOfCity()
    {
        var city = CreateExampleCity1();
        city.Player = 1;
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        // test with given city id
        Assert.IsTrue(cityManager.IsTileOfCity(city.Id, new CubeCoordinates(0, 0, 0)));
        Assert.IsTrue(cityManager.IsTileOfCity(city.Id, new CubeCoordinates(0, 1, -1)));
        Assert.IsFalse(cityManager.IsTileOfCity(917, new CubeCoordinates(0, 0, 0)));
        Assert.IsFalse(cityManager.IsTileOfCity(city.Id, new CubeCoordinates(1, 0, 0)));
        // test without a city id
        Assert.IsTrue(cityManager.IsTileOfCity(new CubeCoordinates(0, 0, 0)));
        Assert.IsTrue(cityManager.IsTileOfCity(new CubeCoordinates(0, 1, -1)));
        Assert.IsFalse(cityManager.IsTileOfCity(new CubeCoordinates(1, 0, 0)));
        // test with ignored player ids
        var playerIds = new List<int>() { 2, 3, 17 };
        Assert.IsFalse(cityManager.IsTileOfCity(new CubeCoordinates(0, 0, 0), playerIds));
        Assert.IsFalse(cityManager.IsTileOfCity(new CubeCoordinates(0, 1, -1), playerIds));
        Assert.IsFalse(cityManager.IsTileOfCity(new CubeCoordinates(1, 0, 0), playerIds));
        playerIds = new List<int>() { 1 };
        Assert.IsTrue(cityManager.IsTileOfCity(new CubeCoordinates(0, 0, 0), playerIds));
        Assert.IsTrue(cityManager.IsTileOfCity(new CubeCoordinates(0, 1, -1), playerIds));
        Assert.IsFalse(cityManager.IsTileOfCity(new CubeCoordinates(1, 0, 0), playerIds));
    }

    [TestMethod]
    public void IsTileACity()
    {
        var city = CreateExampleCity1();
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), new List<BuildingType>(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        Assert.IsTrue(cityManager.IsTileACity(new CubeCoordinates(0, 0, 0)));
        Assert.IsFalse(cityManager.IsTileACity(new CubeCoordinates(0, 1, -1)));
        Assert.IsFalse(cityManager.IsTileACity(new CubeCoordinates(1, 0, 0)));
    }

    [TestMethod]
    public void AddBuilding()
    {
        var city = CreateExampleCity1();
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), CreateBuildingTypes(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        int palace = 1;
        int lumberjack = 2;
        success = cityManager.AddBuilding(city.Id, new CubeCoordinates(1, 0, -1), lumberjack);
        Assert.IsTrue(success, "add first building");
        success = cityManager.AddBuilding(city.Id, new CubeCoordinates(1, 0, -1), lumberjack);
        Assert.IsFalse(success, "already a building at this coordinates");
        success = cityManager.AddBuilding(city.Id, new CubeCoordinates(2, 0, -2), lumberjack);
        Assert.IsFalse(success, "not a city tile");
        success = cityManager.AddBuilding(city.Id, new CubeCoordinates(0, 0, 0), palace);
        Assert.IsTrue(success, "city position for city buildings");
        success = cityManager.AddBuilding(17, new CubeCoordinates(2, 0, -2), lumberjack);
        Assert.IsFalse(success, "city not known");
        success = cityManager.AddBuilding(city.Id, new CubeCoordinates(0, 1, -1), 3);
        Assert.IsFalse(success, "building type unknown");
        success = cityManager.AddBuilding(city.Id, new CubeCoordinates(0, 1, -1), lumberjack);
        Assert.IsTrue(success, "add second building");
    }

    [TestMethod]
    public void GetTileStatus()
    {
        var city = CreateExampleCity1();
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), CreateBuildingTypes(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        int value = cityManager.GetTileStatus(new CubeCoordinates(2, -1, 0));
        Assert.IsTrue(-2 == value, $"Cordinates should be wrong. Return value: {value}.");
        value = cityManager.GetTileStatus(new CubeCoordinates(2, 0, -1));
        Assert.IsTrue((int)TileType.EMPTY == value, $"Tile should be empty. Return value: {value}.");
        value = cityManager.GetTileStatus(new CubeCoordinates(0, 0, 0));
        Assert.IsTrue(city.Id == value, $"Tile should be occupied by a city. Return value: {value}.");
        value = cityManager.GetTileStatus(new CubeCoordinates(1, 0, -1));
        Assert.IsTrue(city.Id == value, $"Tile should be part of city tiles. Return value: {value}.");
    }

    [TestMethod]
    public void GetTilesForGrow()
    {
        var city = CreateExampleCity1();
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), CreateBuildingTypes(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var city2 = CreateExampleCity2();
        success = cityManager.CreateCity(city2);
        Assert.IsTrue(success);
        var possibleTiles = cityManager.GetTilesForGrow(city.Id, 3);
        Assert.AreEqual(6, possibleTiles.Count);
    }

    private CityBase CreateExampleCity1()
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

    private CityBase CreateExampleCity2()
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

    private CityBase CloneCity(CityBase city)
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

    private List<BuildingType> CreateBuildingTypes()
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
