﻿using com.hexagonsimulations.Geometry.Hex;
using com.hexagonsimulations.HexMapCities.Enums;
using com.hexagonsimulations.HexMapCities.Models;
using com.hexagonsimulations.HexMapCities.Tests.Models;

namespace com.hexagonsimulations.HexMapCities.Tests;

[TestClass]
public sealed class CityManagerTests
{
    private readonly int _tileWidth = 34;
    private readonly int _tileHeight = 32;

    [TestMethod]
    public void TestCreateCity()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>());
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
    }

    [TestMethod]
    public void TestCreateCityCollision()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        exampleMap[0] = (int)TileType.UNBUILDABLE;
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>() { (int)TileType.UNBUILDABLE });
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public void TestGetCityById()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>());
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
    public void TestGetCitiesOfPlayer()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>());
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
    public void TestCreateCityBorders()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), _tileWidth, _tileHeight);
        var city = CreateExampleCity1();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        cityManager.CreateCityBorders(city.Player);
        Assert.AreEqual(12, city.Borders.Count);
    }

    [TestMethod]
    public void TestCreateCityAddTilesAndBorders()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), _tileHeight, _tileWidth);
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
    public void TestCreateCityBordersMoreCities()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), _tileWidth, _tileHeight);
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
    public void TestTwoCitiesTouchOnOneLine()
    {
        var exampleMap = Enumerable.Repeat(0, 36).ToList();
        var cityManager = new CityManager(exampleMap, 6, 6, new List<int>(), _tileWidth, _tileHeight);
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
    public void TestAddCityTile()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>());
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
    public void TestAddCityTileNotValid()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>());
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
    public void TestAddCityTileCollisionWithOtherCity()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>());
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
    public void TestCreateCityBordersAfterTileWasAdded()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), _tileWidth, _tileHeight);
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
    public void TestCreateCityBordersMoreCitiesAndAddTile()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), _tileWidth, _tileHeight);
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
    public void TestCreateCityBordersMoreCitiesDashedBorder()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>(), _tileWidth, _tileHeight);
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
    public void TestCityProperties()
    {
        var city = CreateExampleCity1();
        city.Properties.Add("sprite.position", new Vector2() { X = 1.0f, Y = 2.0f });
        city.Properties.Add("world.position", new Vector3() { X = 3.0f, Y = 2.0f, Z = 1.0f });
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), _tileWidth, _tileHeight);
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
    public void TestIsTileOfCity()
    {
        var city = CreateExampleCity1();
        var cityManager = new CityManager(Enumerable.Repeat(0, 16).ToList(), 4, 4, new List<int>(), _tileWidth, _tileHeight);
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        Assert.IsTrue(cityManager.IsTileOfCity(city.Id, new CubeCoordinates(0, 0, 0)));
        Assert.IsTrue(cityManager.IsTileOfCity(city.Id, new CubeCoordinates(0, 1, -1)));
        Assert.IsFalse(cityManager.IsTileOfCity(917, new CubeCoordinates(0, 0, 0)));
        Assert.IsFalse(cityManager.IsTileOfCity(city.Id, new CubeCoordinates(1, 0, 0)));
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
}
