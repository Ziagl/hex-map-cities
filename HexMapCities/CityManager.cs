using com.hexagonsimulations.HexMapBase.Models;
using com.hexagonsimulations.HexMapCities.Enums;
using com.hexagonsimulations.HexMapCities.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.hexagonsimulations.HexMapCities;

public class CityManager
{
    private Dictionary<int, CityBase> _cityStore = new();
    private int _lastCityStoreId = 0;
    private MapData _map = new();
    private int _tileWidth = 0;
    private int _tileHeight = 0;
    private List<BuildingType> _buildingDefinitions = new();

    private CityManager() { }

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
        _buildingDefinitions = buildingDefinitions;
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
    /// <returns>dictionary of all distances and list of tiles that can be used for AddCityTile</returns>
    public Dictionary<int, List<CubeCoordinates>> GetTilesForGrow(int cityId, int maxDistance)
    {
        CityBase? city = null;
        if (!_cityStore.TryGetValue(cityId, out city))
        {
            return new();
        }
        // compute a list of all possible tiles
        var possibleTiles = Utils.GetNeighborsForDistance(city.Position, city.Position, maxDistance);
        // remove all tiles that are already part of any city
        foreach (var otherCity in _cityStore.Values)
        {
            foreach (var tile in otherCity.Tiles)
            {
                possibleTiles.Remove(tile);
            }
            possibleTiles.Remove(otherCity.Position);
        }
        // remove all tiles that are not a neighbor of a city tile
        Dictionary<int, List<CubeCoordinates>> neighbors = new();
        foreach (var tile in city.Tiles)
        {
            foreach (var neighbor in tile.Neighbors())
            {
                int distance = CubeCoordinates.Distance(city.Position, neighbor);

                if (distance == 0)
                {
                    continue;
                }
                if (!neighbors.ContainsKey(distance))
                {
                    neighbors[distance] = new List<CubeCoordinates>();
                }
                if (possibleTiles.Contains(neighbor) && !neighbors[distance].Contains(neighbor))
                {
                    neighbors[distance].Add(neighbor);
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
    /// <param name="coordinates">Coordinates to check</param>
    /// <param name="includeTiles">Flag if also city tiles should be checked</param>
    /// <returns>city if found, null if not found</returns>
    public CityBase? GetCityByCoordinates(CubeCoordinates coordinates, bool includeTiles = false)
    {
        foreach (var cityEntry in _cityStore)
        {
            if (cityEntry.Value.Position == coordinates)
            {
                return cityEntry.Value;
            }
            if (includeTiles)
            {
                foreach (var tile in cityEntry.Value.Tiles)
                {
                    if (tile == coordinates)
                    {
                        return cityEntry.Value;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// All cities of given player number
    /// </summary>
    /// <param name="playerId">player id to search for</param>
    /// <returns>List of cities for given player, if no unit was found, empty array</returns>
    public List<CityBase> GetCitiesOfPlayer(int playerId)
        => FindCities(playerId, null);

    /// <summary>
    /// Find cities that meet given criteria. You can filter by playerId and/or buildingType.
    /// </summary>
    /// <param name="playerId">Player Id the city should belong (can be null).</param>
    /// <param name="buildingType">Type of building this city should have built (can be null).</param>
    /// <returns>List of cities that match the criteria.</returns>
    public List<CityBase> FindCities(int? playerId = null, int? buildingType = null)
        => _cityStore.Values
            .Where(city => 
                (playerId.HasValue || buildingType.HasValue) &&
                (!playerId.HasValue || city.Player == playerId.Value) &&
                (buildingType == null || city.Buildings.Any(b => b.Type == buildingType)))
            .ToList();

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
    /// Tests if given coordinates are part of a city (city position or one of its tiles)
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
    /// <param name="buildingTypeId">building id that should be built</param>
    /// <returns>true if building was built, false otherwise</returns>
    public bool AddBuilding(int cityId, CubeCoordinates coordinates, int buildingTypeId)
    {
        var city = GetCityById(cityId);
        // early exit is city was not found
        if (city == null)
        {
            return false;
        }
        // early exit if buildingTypeId is not valid
        if(buildingTypeId < 1 || buildingTypeId > _buildingDefinitions.Count)
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
        var building = BuildingFactory.CreateBuilding(_buildingDefinitions[buildingTypeId-1]);
        if(building is null)
        {
            return false;
        }
        building.Position = coordinates;
        city.Buildings.Add(building);
        return true;
    }

    /// <summary>
    /// Adds an inhabitant to a city.
    /// </summary>
    /// <param name="cityId">id of city</param>
    /// <param name="inhabitant">already generated inhabitant instance</param>
    /// <returns>true if inhabitant was added otherwise false</returns>
    public bool AddInhabitant(int cityId, InhabitantBase inhabitant)
    {
        var city = GetCityById(cityId);
        // early exit is city was not found
        if (city == null)
        {
            return false;
        }
        // exit if inhabitant position is not part of this city
        if (!IsTileOfCity(cityId, inhabitant.Position))
        {
            return false;
        }
        // exit if there are not enough free dwellings on position
        var building = GetBuildingOfPosition(inhabitant.Position);
        if (building is null)
        {
            return false;   // no building on this position
        }
        var otherInhabitants = GetInhabitantsOfPosition(inhabitant.Position);
        if (building.Citizens - otherInhabitants.Count <= 0)
        {
            return false;   // not enough free dwellings
        }
        city.Inhabitants.Add(inhabitant);
        return true;
    }

    /// <summary>
    /// Retrieves the building located at the specified coordinates, if any.
    /// </summary>
    /// <param name="coordinates">Coordinates to check</param>
    /// <returns>The <see cref="BuildingBase"/> instance located at the specified coordinates, or <see langword="null"/>  if no
    /// building exists at the given position.</returns>
    public BuildingBase? GetBuildingOfPosition(CubeCoordinates coordinates)
    {
        foreach(var city in _cityStore.Values)
        {
            foreach(var building in city.Buildings)
            {
                if(building.Position == coordinates)
                {
                    return building;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Gets a list of inhabitants of the given coordinates.
    /// </summary>
    /// <param name="coordinates">Coordinates to check</param>
    /// <returns>list of found inhabitants</returns>
    public List<InhabitantBase> GetInhabitantsOfPosition(CubeCoordinates coordinates)
    {
        List<InhabitantBase> inhabitants = new();
        foreach(var city in _cityStore.Values)
        {
            foreach(var inhabitant in city.Inhabitants)
            {
                if(inhabitant.Position == coordinates)
                {
                    inhabitants.Add(inhabitant);
                }
            }
        }
        return inhabitants;
    }

    private sealed class CityManagerState
    {
        public int LastCityStoreId { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public MapData MapData { get; set; } = new();
        public Dictionary<int, CityBase> CityStore { get; set; } = new();
        public List<BuildingType> BuildingDefinitions { get; set; } = new();
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Preserve references in case objects (like coordinates) are reused.
        ReferenceHandler = ReferenceHandler.Preserve,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serialize current CityManager state to JSON.
    /// </summary>
    public string ToJson(JsonSerializerOptions? options = null)
    {
        var state = new CityManagerState
        {
            LastCityStoreId = _lastCityStoreId,
            TileWidth = _tileWidth,
            TileHeight = _tileHeight,
            MapData = _map,
            CityStore = _cityStore,
            BuildingDefinitions = _buildingDefinitions
        };
        return JsonSerializer.Serialize(state, options ?? _jsonOptions);
    }

    /// <summary>
    /// Deserialize a CityManager from JSON.
    /// </summary>
    public static CityManager FromJson(string json, JsonSerializerOptions? options = null)
    {
        var state = JsonSerializer.Deserialize<CityManagerState>(json, options ?? _jsonOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize CityManagerState.");
        var manager = new CityManager
        {
            _lastCityStoreId = state.LastCityStoreId,
            _tileWidth = state.TileWidth,
            _tileHeight = state.TileHeight,
            _map = state.MapData ?? new MapData(),
            _cityStore = state.CityStore ?? new(),
            _buildingDefinitions = state.BuildingDefinitions ?? new()
        };
        return manager;
    }

    /// <summary>
    /// Writes the state of this CityManager instance to a BinaryWriter.
    /// A version number is written first to allow future format evolution.
    /// Currently the payload is the JSON representation (length prefixed UTF-8).
    /// </summary>
    public void Write(BinaryWriter writer)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));

        const int formatVersion = 1;
        writer.Write(formatVersion);

        var json = ToJson();
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>
    /// Reads a CityManager instance from a BinaryReader created by Write.
    /// </summary>
    public static CityManager Read(BinaryReader reader)
    {
        if (reader is null) throw new ArgumentNullException(nameof(reader));

        int formatVersion = reader.ReadInt32();
        if (formatVersion != 1)
        {
            throw new NotSupportedException($"Unsupported CityManager binary format version {formatVersion}.");
        }

        int length = reader.ReadInt32();
        if (length < 0) throw new InvalidDataException("Negative payload length encountered.");

        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
        {
            throw new EndOfStreamException("Unexpected end of stream while reading CityManager payload.");
        }

        var json = System.Text.Encoding.UTF8.GetString(bytes);
        return FromJson(json);
    }
}
