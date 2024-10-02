using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HexMapCities.Tests")]

namespace HexMapCities.Models;

internal class Utils
{
    // creates border lines for a city by given tile pixel coordinates and width/height of tile
    internal static void CreateBordersForCity(CityBase city, int tileWidth, int tileHeight)
    {
        List<Line> borderLines = new();
        // add tile borders for city center
        borderLines.AddRange(Utils.ComputeBordersOfTile(city.PositionPixel, tileWidth, tileHeight));
        // add tile borders for all city tiles
        foreach(var tile in city.TilesPixel)
        {
            var tileBorders = Utils.ComputeBordersOfTile(tile, tileWidth, tileHeight);
            foreach(var tileBorder in tileBorders)
            {
                bool inList = false;
                // if tile borders already in list, remove them
                for(int i = 0; i < borderLines.Count; i++)
                {
                    if (borderLines[i].Equals(tileBorder))
                    {
                        borderLines.RemoveAt(i);
                        inList = true;
                        break;
                    }
                }
                // add tile borders if not already in list
                if(!inList)
                {
                    borderLines.Add(tileBorder);
                }
            }
        }
        city.Borders = borderLines;
    }

    // removes all duplicated border lines of all cities
    internal static void RemoveDuplicateBorders(List<CityBase> cities)
    {
        // create a list of all border lines of all cities
        List<(int cityId, Line borderLine)> allBorders = new();
        foreach (var city in cities)
        {
            foreach (var border in city.Borders)
            {
                allBorders.Add((city.Id, border));
            }
            city.Borders.Clear();
        }
        // find border lines that are unique
        List<(int cityId, Line borderLine)> uniqueBorders = new();
        for(int i = 0; i < allBorders.Count; ++i)
        {
            bool isUnique = true;
            for (int j = 0; j < allBorders.Count; ++j)
            {
                if(i == j)
                {
                    continue;
                }
                if (allBorders[i].borderLine.Equals(allBorders[j].borderLine))
                {
                    isUnique = false;
                    break;
                }
            }
            if (isUnique)
            {
                uniqueBorders.Add(allBorders[i]);
            }
        }
        // refill borders of all cities
        foreach (var city in cities)
        {
            foreach(var border in allBorders)
            {
                if (border.cityId == city.Id)
                {
                    city.Borders.Add(border.borderLine);
                }
            }
        }
    }

    // computes border lines of a hexagon tile by given center pixel coordinates and width/height of tile
    internal static List<Line> ComputeBordersOfTile(Point tileOrigin, int tileWidth, int tileHeight)
    {
        List<Line> borderLines = new();
        var tileCenter = new Point(tileOrigin.X + tileWidth / 2, tileOrigin.Y + tileHeight / 2);
        // NW
        var startPoint = new Point(tileOrigin.X - tileWidth / 2, tileOrigin.Y - tileHeight / 4);
        var endPoint = new Point(tileOrigin.X, tileOrigin.Y - tileHeight / 2);
        borderLines.Add(new Line(startPoint, endPoint));
        // NE
        startPoint = endPoint;
        endPoint = new Point(tileOrigin.X + tileWidth / 2, tileOrigin.Y - tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        // E
        startPoint = endPoint;
        endPoint = new Point(tileOrigin.X + tileWidth / 2, tileOrigin.Y + tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        // SE
        startPoint = endPoint;
        endPoint = new Point(tileOrigin.X, tileOrigin.Y + tileHeight / 2);
        borderLines.Add(new Line(startPoint, endPoint));
        // SW
        startPoint = endPoint;
        endPoint = new Point(tileOrigin.X - tileWidth / 2, tileOrigin.Y + tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        // W
        startPoint = endPoint;
        endPoint = new Point(tileOrigin.X - tileWidth / 2, tileOrigin.Y - tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        return borderLines;
    }
}
