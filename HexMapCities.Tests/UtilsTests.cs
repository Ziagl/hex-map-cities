using HexMapCities.Models;

namespace HexMapCities.Tests;

public class UtilsTests
{
    [Fact]
    public void TestComputeBordersOfTile()
    {
        int tileWidth = 34;
        int tileHeight = 32;
        var output = Utils.ComputeBordersOfTile(new Point(), tileWidth, tileHeight);
        Assert.Equal(6, output.Count);
        Assert.Equal(new Line(new Point(-17, -8), new Point(0, -16)), output[0]);
        Assert.Equal(new Line(new Point(0, -16), new Point(17, -8)), output[1]);
        Assert.Equal(new Line(new Point(17, -8), new Point(17, 8)), output[2]);
        Assert.Equal(new Line(new Point(17, 8), new Point(0, 16)), output[3]);
        Assert.Equal(new Line(new Point(0, 16), new Point(-17, 8)), output[4]);
        Assert.Equal(new Line(new Point(-17, 8), new Point(-17, -8)), output[5]);
    }
}
