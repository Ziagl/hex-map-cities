using com.hexagonsimulations.Geometry.Hex;
using HexMapCities.Models;

namespace HexMapCities.Tests;

public class UtilsTests
{
    [Fact]
    public void TestComputeBordersOfTile()
    {
        int tileWidth = 34;
        int tileHeight = 32;
        var output = Utils.ComputeBordersOfTile(new Point(-(tileWidth / 2), -(tileHeight / 2)), tileWidth, tileHeight);
        Assert.Equal(6, output.Count);
        Assert.Equal(new Line(new Point(-17, -8), new Point(0, -16)), output[0]);
        Assert.Equal(new Line(new Point(0, -16), new Point(17, -8)), output[1]);
        Assert.Equal(new Line(new Point(17, -8), new Point(17, 8)), output[2]);
        Assert.Equal(new Line(new Point(17, 8), new Point(0, 16)), output[3]);
        Assert.Equal(new Line(new Point(0, 16), new Point(-17, 8)), output[4]);
        Assert.Equal(new Line(new Point(-17, 8), new Point(-17, -8)), output[5]);
    }

    [Fact]
    public void TestIsCityNeighbor()
    {
        var city = CreateExampleCity1();
        bool neighbor = Utils.IsCityNeighbor(city, new CubeCoordinates(2, 0, -2));
        Assert.True(neighbor);
        neighbor = Utils.IsCityNeighbor(city, new CubeCoordinates(3, 0, -3));
        Assert.False(neighbor);
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
