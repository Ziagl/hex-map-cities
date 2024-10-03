﻿using com.hexagonsimulations.Geometry.Hex;
using HexMapCities.Enums;
using HexMapCities.Models;

namespace HexMapCities;

public class CityManager
{
    private Dictionary<int, CityBase> _cityStore = new();
    private int _lastCityStoreId = 0;
    private MapData _map = new();

    public CityManager(List<int> map, int rows, int columns, List<int> notPassableTiles)
    {
        _map.Rows = rows;
        _map.Columns = columns;

        List<int> layerMap = new();
        foreach (var tile in map)
        {
            if (notPassableTiles.Contains(tile))
            {
                layerMap.Add((int)TileType.UNBUILDABLE);
            }
            else
            {
                layerMap.Add((int)TileType.EMPTY);
            }
        }

        _map.Map = layerMap;
    }

    /// <summary>
    /// Creates a new city, returns false if not possible.
    /// </summary>
    /// <param name="city">city to create</param>
    /// <returns>true if city was created, false if position is already occupied</returns>
    public bool CreateCity(CityBase city)
    {
        // early exit if layer position is already occupied
        var coordinates = city.Position.ToOffset();
        var unitId = _map.Map[coordinates.y * _map.Columns + coordinates.x];
        if (unitId != (int)TileType.EMPTY)
        {
            return false;
        }
        // add city to store
        _lastCityStoreId++;
        city.Id = _lastCityStoreId;
        _cityStore.Add(city.Id, city);
        _map.Map[coordinates.y * _map.Columns + coordinates.x] = city.Id;
        return true;
    }

    /// <summary>
    /// Creates all surrounding border lines of all city tiles and saves result for city object for all cities
    /// </summary>
    /// <param name="playerId">id of player to create borders for</param>
    /// <param name="tileWidth">width of tile in pixel</param>
    /// <param name="tileHeight">height of tile in pixel</param>
    public void CreateCityBorders(int playerId, int tileWidth, int tileHeight)
    {
        var playerCities = GetCitiesOfPlayer(playerId);
        // create all borders for all cities individually
        foreach (var city in playerCities)
        {
            Utils.CreateBordersForCity(city, tileWidth, tileHeight);
        }
        // remove all duplicated borders (overlapping cities)
        Utils.RemoveDuplicateBorders(playerCities);
    }

    /// <summary>
    /// Adds a new tile to a city, returns false if not possible
    /// </summary>
    /// <param name="cityId">id of city that should grow</param>
    /// <param name="tile">new tile of city</param>
    /// <param name="tilePixel">pixel position of tile</param>
    /// <returns>true if tile was added, false if tile is already part of city or not a valid neighbor</returns>
    public bool AddCityTile(int cityId, CubeCoordinates tile, Point tilePixel)
    {
        CityBase? city = null;
        if (!_cityStore.TryGetValue(cityId, out city))
        {
            return false;
        }
        // early exit if tile is city position
        if(city.Position == tile)
        {
            return false;
        }
        // early exit if tile is already part of city
        if(city.Tiles.Contains(tile))
        {
            return false;
        }
        // check if given tile is a valid neighbor of city
        if(!Utils.IsCityNeighbor(city, tile))
        {
            return false;
        }
        // check if tile is not already occupied by another city
        foreach (var otherCity in _cityStore.Values)
        {
            if (city.Id == otherCity.Id)
            {
                continue;
            }
            if (city.Position == otherCity.Position || otherCity.Tiles.Contains(tile))
            {
                return false;
            }
        }
        // add tile to city
        city.Tiles.Add(tile);
        city.TilesPixel.Add(tilePixel);
        return true;
    }

    /// <summary>
    /// Get city by id
    /// </summary>
    /// <param name="cityId">id of city</param>
    /// <returns>city if found, undefined if not found</returns>
    public CityBase? GetCityById(int cityId)
    {
        CityBase? city = null;
        if (_cityStore.TryGetValue(cityId, out city))
        {
            return city;
        }
        return null;
    }

    /// <summary>
    /// All cities of given player number
    /// </summary>
    /// <param name="playerId">player id to search for</param>
    /// <returns>array of cities for given player, if no unit was found, empty array</returns>
    public List<CityBase> GetCitiesOfPlayer(int playerId)
    {
        List<CityBase> foundCities = new();
        foreach (var city in _cityStore)
        {
            if (city.Value.Player == playerId)
            {
                foundCities.Add(city.Value);
            }
        }
        return foundCities;
    }
}