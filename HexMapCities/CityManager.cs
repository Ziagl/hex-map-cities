using com.hexagonsimulations.HexMapBase.Models;
using com.hexagonsimulations.HexMapCities.Enums;
using com.hexagonsimulations.HexMapCities.Models;

namespace com.hexagonsimulations.HexMapCities;

public class CityManager
{
    private Dictionary<int, CityBase> _cityStore = new();
    private int _lastCityStoreId = 0;
    private MapData _map = new();
    private int _tileWidth = 0;
    private int _tileHeight = 0;
    private BuildingFactory _factory;

    public CityManager(List<int> map, int rows, int columns, List<int> notPassableTiles, List<BuildingType> buildingDefinitions, int tileWidth = 1, int tileHeight = 1)
    {
        _map.Rows = rows;
        _map.Columns = columns;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;

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
        _factory = new BuildingFactory(buildingDefinitions);
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
    public void CreateCityBorders(int playerId)
    {
        var playerCities = GetCitiesOfPlayer(playerId);
        // create all borders for all cities individually
        foreach (var city in playerCities)
        {
            Utils.CreateBordersForCity(city, _tileWidth, _tileHeight);
        }
        // remove all duplicated borders (overlapping cities)
        Utils.RemoveDuplicateBorders(playerCities);
        // update dashed borders
        Utils.UpdateDashedBorders(new List<CityBase>(_cityStore.Values));
    }

    /// <summary>
    /// Adds a new tile to a city, returns false if not possible
    /// </summary>
    /// <param name="cityId">id of city that should grow</param>
    /// <param name="tile">new tile of city</param>
    /// <returns>true if tile was added, false if tile is already part of city or not a valid neighbor</returns>
    public bool AddCityTile(int cityId, CubeCoordinates tile)
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
        var tilePixel = Utils.ComputeTilePixel(city.PositionPixel, city.Position, tile, _tileWidth, _tileHeight);
        // check if tile pixel was computed successfully
        if(tilePixel is null)
        {
            return false;
        }
        // add tile to city
        city.Tiles.Add(tile);
        city.TilesPixel.Add(tilePixel);
        return true;
    }

    /// <summary>
    /// Get all tiles that a city can grow to
    /// </summary>
    /// <param name="cityId">Id of city that should grow</param>
    /// <returns>list of tiles that can be used for AddCityTile</returns>
    public List<CubeCoordinates> GetTilesForGrow(int cityId, int maxDistance)
    {
        CityBase? city = null;
        if (!_cityStore.TryGetValue(cityId, out city))
        {
            return new();
        }
        // compute a list of all possible tiles
        var possibleTiles = Utils.GetNeighborsForDistance(city.Position, city.Position, maxDistance);
        // remove all tiles that are already part of any city
        foreach(var otherCity in _cityStore.Values)
        {
            foreach (var tile in otherCity.Tiles)
            {
                possibleTiles.Remove(tile);
            }
            possibleTiles.Remove(otherCity.Position);
        }
        // remove all tiles that are not a neighbot of a city tile
        List<CubeCoordinates> neighbors = new();
        foreach(var tile in city.Tiles)
        {
            foreach(var neighbor in tile.Neighbors())
            {
                if(possibleTiles.Contains(neighbor) && !neighbors.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
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
    /// Get city by coordinates
    /// </summary>
    /// <param name="corrdinates">Coordinates to check</param>
    /// <returns>city if found, undefined if not found</returns>
    public CityBase? GetCityByCoordinates(CubeCoordinates coordinates)
    {
        foreach(var cityEntry in _cityStore)
        {
            if (cityEntry.Value.Position == coordinates)
            {
                return cityEntry.Value;
            }
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

    /// <summary>
    /// Tests if given coordinates are part of given city (city tile or one of its tiles)
    /// </summary>
    /// <param name="cityId">id of city</param>
    /// <param name="coordinates">coordinates of city tile to check</param>
    /// <returns>true if it is part of the city, otherwise false</returns>
    public bool IsTileOfCity(int cityId, CubeCoordinates coordinates)
    {
        var city = GetCityById(cityId);
        // early exit is city was not found
        if (city == null)
        {
            return false;
        }
        if (city.Position == coordinates)
        {
            return true;
        }
        foreach(var tile in city.Tiles)
        {
            if(tile == coordinates)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Tests if given coordinates are part of a city (city pisition or one of its tiles)
    /// </summary>
    /// <param name="coordinates">coordinates of city position to check</param>
    /// <param name="playerIds">optional list of players ids that cities should have</param>
    /// <returns>true if it is part of any city, otherwise false</returns>
    public bool IsTileOfCity(CubeCoordinates coordinates, List<int>? playerIds = null)
    {
        foreach (var city in _cityStore.Values)
        {
            if(playerIds is not null)
            {
                if(!playerIds.Contains(city.Player))
                {
                    return false;
                }
            }
            if (city.Position == coordinates)
            {
                return true;
            }
            foreach (var tile in city.Tiles)
            {
                if (tile == coordinates)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Tests if given coordinates is a city (only check city position)
    /// </summary>
    /// <param name="coordinates">coordinates of city position to check</param>
    /// <returns>true if it is a city, otherwise false</returns>
    public bool IsTileACity(CubeCoordinates coordinates)
    {
        return GetCityByCoordinates(coordinates) != null;
    }

    /// <summary>
    /// Get tile status of given coordinates.
    /// </summary>
    /// <param name="coordinates">Map coordinates</param>
    /// <returns>Returns ether TileType, the cityId this tile belongs 
    /// or -2 if given coordinates are not on map.</returns>
    public int GetTileStatus(CubeCoordinates coordinates)
    {
        var offsetCoordinates = coordinates.ToOffset();
        // early exit if given coordinates are not on map
        if (offsetCoordinates.x < 0 || offsetCoordinates.x >= _map.Columns ||
            offsetCoordinates.y < 0 || offsetCoordinates.y >= _map.Rows)
        {
            return -2;
        }
        var tile = _map.Map[offsetCoordinates.y * _map.Columns + offsetCoordinates.x];
        if(tile == (int)TileType.EMPTY)
        {
            // if tile is empty, it may be a city tile
            foreach (var city in _cityStore)
            {
                foreach(var cityTile in city.Value.Tiles)
                {
                    if (cityTile == coordinates)
                    {
                        return city.Key;
                    }
                }
            }
        }
        return tile;
    }

    /// <summary>
    /// Add given building to given city at given position.
    /// </summary>
    /// <param name="cityId">id of city</param>
    /// <param name="coordinates">coordinates of city tile the building should be placed</param>
    /// <param name="building">building that should be built</param>
    /// <returns></returns>
    public bool AddBuilding(int cityId, CubeCoordinates coordinates, int buildingTypeId)
    {
        var city = GetCityById(cityId);
        // early exit is city was not found
        if (city == null)
        {
            return false;
        }
        // check if given coordinates are city tiles
        bool cityTile = false;
        foreach (var tile in city.Tiles)
        {
            if (tile == coordinates)
            {
                cityTile = true;
                break;
            }
        }
        // skip if coordinates are not city position or any of its tiles
        if (!(cityTile || city.Position == coordinates))
        {
            return false;
        }
        // check if there is not already a building
        bool buildingOnSameTile = false;
        foreach (var cityBuilding in city.Buildings)
        {
            // skip if this is a city building
            if (cityBuilding.Position == city.Position)
            {
                continue;
            }
            // check all city tiles if they are not already occupied by a building
            if (cityBuilding.Position == coordinates)
            {
                buildingOnSameTile = true;
                break;
            }
        }
        if(buildingOnSameTile)
        {
            return false;
        }
        // check if building type is known
        var building = _factory.CreateBuilding(buildingTypeId);
        if(building is null)
        {
            return false;
        }
        building.Position = coordinates;
        city.Buildings.Add(building);
        return true;
    }
}
