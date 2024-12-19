using com.hexagonsimulations.HexMapBase.Geometry.Hex;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HexMapCities.Tests")]

namespace com.hexagonsimulations.HexMapCities.Models;

internal class Utils
{
    // for pixel logic with floats it is ok if they differ up to 1 pixel (min rounding error)
    internal static float PRECISION = 1.0001f;

    // creates border lines for a city by given tile pixel coordinates and width/height of tile
    internal static void CreateBordersForCity(CityBase city, int tileWidth, int tileHeight)
    {
        List<Line> borderLines = new();
        // add tile borders for city center
        borderLines.AddRange(Utils.ComputeBordersOfTile(city.PositionPixel, tileWidth, tileHeight));
        // add tile borders for all city tiles
        foreach (var tile in city.TilesPixel)
        {
            var tileBorders = Utils.ComputeBordersOfTile(tile, tileWidth, tileHeight);
            foreach (var tileBorder in tileBorders)
            {
                bool inList = false;
                // if tile borders already in list, remove them
                for (int i = 0; i < borderLines.Count; i++)
                {
                    if (borderLines[i] == tileBorder)
                    {
                        borderLines.RemoveAt(i);
                        inList = true;
                        break;
                    }
                }
                // add tile borders if not already in list
                if (!inList)
                {
                    borderLines.Add(tileBorder);
                }
            }
        }
        city.Borders = borderLines;
    }

    // analysis all overlapping borderlines of adjacent cities
    // of different players and sets dached attribute to border
    internal static void UpdateDashedBorders(List<CityBase> cities)
    {
        // reset dashed flags
        for(int i = 0; i < cities.Count; ++i)
        {
            for(int j = 0; j < cities[i].Borders.Count; ++j)
            {
                cities[i].Borders[j].Dashed = false;
            }
        }
        // set dashed flag
        for(int i = 0; i < cities.Count; ++i)
        {
            for(int j = 0; j < cities.Count; ++j)
            {
                if(i == j)
                {
                    continue;
                }
                if (cities[i].Player == cities[j].Player)
                {
                    continue;
                }
                for(int ii = 0; ii < cities[i].Borders.Count; ++ii)
                {
                    for(int jj = 0; jj < cities[j].Borders.Count; ++jj)
                    {
                        if ((cities[i].Borders[ii].Start == cities[j].Borders[jj].Start && 
                            cities[i].Borders[ii].End == cities[j].Borders[jj].End) ||
                            (cities[i].Borders[ii].Start == cities[j].Borders[jj].End &&
                            cities[i].Borders[ii].End == cities[j].Borders[jj].Start))
                        {
                            if (cities[i].Player > cities[j].Player)
                            {
                                cities[i].Borders[ii].Dashed = true;
                            }
                            else
                            {
                                cities[j].Borders[jj].Dashed = true;
                            }
                        }
                    }
                }
            }
        }
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
        for (int i = 0; i < allBorders.Count; ++i)
        {
            bool isUnique = true;
            for (int j = 0; j < allBorders.Count; ++j)
            {
                if (i == j)
                {
                    continue;
                }
                if (allBorders[i].borderLine == allBorders[j].borderLine)
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
            foreach (var border in uniqueBorders)
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
        var startPoint = new Point(tileCenter.X - tileWidth / 2, tileCenter.Y - tileHeight / 4);
        var endPoint = new Point(tileCenter.X, tileCenter.Y - tileHeight / 2);
        borderLines.Add(new Line(startPoint, endPoint));
        // NE
        startPoint = endPoint;
        endPoint = new Point(tileCenter.X + tileWidth / 2, tileCenter.Y - tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        // E
        startPoint = endPoint;
        endPoint = new Point(tileCenter.X + tileWidth / 2, tileCenter.Y + tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        // SE
        startPoint = endPoint;
        endPoint = new Point(tileCenter.X, tileCenter.Y + tileHeight / 2);
        borderLines.Add(new Line(startPoint, endPoint));
        // SW
        startPoint = endPoint;
        endPoint = new Point(tileCenter.X - tileWidth / 2, tileCenter.Y + tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        // W
        startPoint = endPoint;
        endPoint = new Point(tileCenter.X - tileWidth / 2, tileCenter.Y - tileHeight / 4);
        borderLines.Add(new Line(startPoint, endPoint));
        return borderLines;
    }

    // checks if given tile is neighbot of any city tile
    internal static bool IsCityNeighbor(CityBase city, CubeCoordinates tile)
    {
        // check if tile is direct neighbor of city
        if (city.Position.Neighbors().Contains(tile))
        {
            return true;
        }

        // check if tile is neighbor of any city tile
        foreach (var cityTile in city.Tiles)
        {
            if (cityTile.Neighbors().Contains(tile))
            {
                return true;
            }
        }

        return false;
    }

    // calculates pixel position of tile
    internal static Point? ComputeTilePixel(Point cityPixel, CubeCoordinates cityPosition, CubeCoordinates tile, int tileWidth, int tileHeight, int depth = 0, List<CubeCoordinates>? alreadyVisited = null, int maxDepth = 5)
    {
        if (alreadyVisited is null)
        {
            alreadyVisited = new() { cityPosition };
        }

        // if tile is neighbor, we can directly calculate
        CubeCoordinates diff = tile - cityPosition;
        var neighbors = new List<CubeCoordinates>()
        {
            new CubeCoordinates(0, -1, 1),  // NW
            new CubeCoordinates(1, -1, 0),  // NO
            new CubeCoordinates(1, 0, -1),  // O
            new CubeCoordinates(0, 1, -1),  // SO
            new CubeCoordinates(-1, 1, 0),  // SW
            new CubeCoordinates(-1, 0, 1),  // W
        };
        var pixelChanges = new List<Point>()
        {
            new Point(-tileWidth / 2,  -tileHeight * 0.75f),
            new Point(tileWidth / 2, -tileHeight * 0.75f),
            new Point(tileWidth,0),
            new Point(tileWidth / 2, tileHeight * 0.75f),
            new Point(-tileWidth / 2, tileHeight * 0.75f),
            new Point(-tileWidth,0),
        };

        for (int i = 0; i < neighbors.Count; ++i)
        {
            if (alreadyVisited.Contains(neighbors[i] + cityPosition))
            {
                continue;
            }
            
            if (diff.q == neighbors[i].q && diff.r == neighbors[i].r && diff.s == neighbors[i].s)
            {
                return new Point(cityPixel.X + pixelChanges[i].X, cityPixel.Y + pixelChanges[i].Y);
            }
            else
            {
                alreadyVisited.Add(neighbors[i] + cityPosition);
            }
        }

        // if not, we can do a recursive search
        ++depth;
        if(depth < maxDepth)
        {
            for(int i = 0; i < neighbors.Count; ++i)
            {
                var newCityPixel = new Point(cityPixel.X + pixelChanges[i].X, cityPixel.Y + pixelChanges[i].Y);
                var newCityPosition = cityPosition + neighbors[i];
                var result = ComputeTilePixel(newCityPixel, newCityPosition, tile, tileWidth, tileHeight, depth, alreadyVisited);
                if(result is not null)
                {
                    return result;
                }
            }
        }

        return null;
    }
}
