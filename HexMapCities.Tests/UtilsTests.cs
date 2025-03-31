using com.hexagonsimulations.HexMapBase.Models;
using com.hexagonsimulations.HexMapCities.Models;

namespace com.hexagonsimulations.HexMapCities.Tests;

[TestClass]
public sealed class UtilsTests
{
    [TestMethod]
    public void TestComputeBordersOfTile()
    {
        int tileWidth = 34;
        int tileHeight = 32;
        var output = Utils.ComputeBordersOfTile(new Point(-(tileWidth / 2), -(tileHeight / 2)), tileWidth, tileHeight);
        Assert.AreEqual(6, output.Count);
        Assert.AreEqual(new Line(new Point(-17, -8), new Point(0, -16)), output[0]);
        Assert.AreEqual(new Line(new Point(0, -16), new Point(17, -8)), output[1]);
        Assert.AreEqual(new Line(new Point(17, -8), new Point(17, 8)), output[2]);
        Assert.AreEqual(new Line(new Point(17, 8), new Point(0, 16)), output[3]);
        Assert.AreEqual(new Line(new Point(0, 16), new Point(-17, 8)), output[4]);
        Assert.AreEqual(new Line(new Point(-17, 8), new Point(-17, -8)), output[5]);
    }

    [TestMethod]
    public void TestIsCityNeighbor()
    {
        var city = CreateExampleCity1();
        bool neighbor = Utils.IsCityNeighbor(city, new CubeCoordinates(2, 0, -2));
        Assert.IsTrue(neighbor);
        neighbor = Utils.IsCityNeighbor(city, new CubeCoordinates(3, 0, -3));
        Assert.IsFalse(neighbor);
    }

    [TestMethod]
    public void TestComputeTilePixel()
    {
        int tileWidth = 34;
        int tileHeight = 32;
        var cityPosition = new CubeCoordinates(1, 1, -2);
        var cityPixel = new Point(51, 24);
        // easy tests
        var tilePositions = new List<CubeCoordinates>()
        {
            new CubeCoordinates(1, 0, -1),
            new CubeCoordinates(2, 0, -2),
            new CubeCoordinates(2, 1, -3),
            new CubeCoordinates(1, 2, -3),
            new CubeCoordinates(0, 2, -2),
            new CubeCoordinates(0, 1, -1),
        };
        var results = new List<Point>()
        {
            new Point(34,0),
            new Point(68,0),
            new Point(85,24),
            new Point(68,48),
            new Point(34,48),
            new Point(17,24),
        };
        for (int i = 0; i < tilePositions.Count; ++i)
        {
            var tilePixel = Utils.ComputeTilePixel(cityPixel, cityPosition, tilePositions[i], tileWidth, tileHeight);
            Assert.IsNotNull(tilePixel);
            Assert.AreEqual(results[i].X, tilePixel.X);
            Assert.AreEqual(results[i].Y, tilePixel.Y);
        }
        // advanced tests
        tilePositions = new List<CubeCoordinates>()
        {
            new CubeCoordinates(0, 0, 0),
            new CubeCoordinates(3, 0, -3),
            new CubeCoordinates(4, 1, -5),
            new CubeCoordinates(4, 2, -6)
        };
        results = new List<Point>()
        {
            new Point(0,0),
            new Point(102,0),
            new Point(153,24),
            new Point(170,48),
        };
        for (int i = 0; i < tilePositions.Count; ++i)
        {
            var tilePixel = Utils.ComputeTilePixel(cityPixel, cityPosition, tilePositions[i], tileWidth, tileHeight);
            Assert.IsNotNull(tilePixel);
            Assert.AreEqual(results[i].X, tilePixel.X);
            Assert.AreEqual(results[i].Y, tilePixel.Y);
        }
    }

    [TestMethod]
    public void TestGetNeighborsForDistance()
    {
        var city = CreateExampleCity1();
        var neighbors = Utils.GetNeighborsForDistance(city.Position, city.Position, 1);
        Assert.AreEqual(6, neighbors.Count);
        neighbors = Utils.GetNeighborsForDistance(city.Position, city.Position, 2);
        Assert.AreEqual(18, neighbors.Count);
        neighbors = Utils.GetNeighborsForDistance(city.Position, city.Position, 3);
        Assert.AreEqual(36, neighbors.Count);
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
}