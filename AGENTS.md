# AGENTS.md - HexMapCities Project Guide

## Project Overview

**HexMapCities** is a .NET library (v0.5.4) providing data structures and logic for managing cities in tile-based hexagon games. It's published as a NuGet package by Hexagon Simulations.

- **Namespace**: `com.hexagonsimulations.HexMapCities`
- **Target Framework**: .NET 10.0
- **Dependencies**: HexMapBase v0.5.0 (external hexagon geometry library)
- **Repository**: https://github.com/Ziagl/hex-map-cities
- **Test Framework**: MSTest
- **Company**: Hexagon Simulations
- **Author**: Werner Ziegelwanger
- **Tags**: hexagon, geometry, map, city, tile, 2d, library

## Architecture

### Core Components

1. **CityManager** (`CityManager.cs`) - Main API entry point
   - Manages all cities on a hexagonal tile map
   - Handles city creation, expansion, building placement, and inhabitants
   - Provides serialization (JSON and binary) for save/load functionality
   - Uses internal Dictionary storage with auto-incrementing IDs
   - Thread-safe design not guaranteed - external synchronization required for concurrent access

2. **Models** (`Models/` folder)
   - `CityBase`: City entity with position, tiles, buildings, inhabitants, borders, combat stats
   - `BuildingBase`: Building instance (inherits from `BuildingType`)
   - `BuildingType`: Building definition/configuration with costs, yields, requirements
   - `InhabitantBase`: Citizen with needs, satisfaction, position, upgrade/downgrade logic
   - `InhabitantNeed`: Periodic need system with satisfaction penalties
   - `MapData`: Internal map representation (rows, columns, tile array)
   - `Point`: 2D pixel coordinates for UI rendering
   - `Line`: Border line segments for city boundaries
   - `Utils`: Internal helper methods for borders, neighbors, pixel calculations

3. **Factories**
   - `CityFactory`: Creates `CityBase` instances (name, player)
   - `BuildingFactory`: Creates `BuildingBase` from `BuildingType` definitions

4. **Enums**
   - `TileType`: Internal enum (UNBUILDABLE = -1, EMPTY = 0)

### Design Patterns

- **Factory Pattern**: `CityFactory` and `BuildingFactory` for controlled object creation
- **Internal Constructors**: `CityBase` and `BuildingBase` have internal constructors to enforce factory usage
- **Manager Pattern**: `CityManager` is the central coordinator
- **Serialization Support**: JSON and binary serialization with versioning
- **Record Types**: `BuildingType` and `BuildingBase` use record types for immutability

## Key Concepts

### Coordinate Systems
- **CubeCoordinates**: 3D hex coordinates (q, r, s) from HexMapBase dependency
  - Where q + r + s = 0 (constraint of cube coordinates)
  - Provides methods: `Distance()`, `Neighbors()`, `ToOffset()`
- **OffsetCoordinates**: 2D array indices (x, y) for internal map storage
- **Pixel Coordinates**: Absolute screen positions for rendering (Point type)
  - Uses float values for precision
  - Computed from hex coordinates using tile width/height

### City Territory
- Cities have a **position** (center hex) and expandable **tiles** (additional hexes)
- Tiles must be neighbors of existing city tiles (checked via `IsCityNeighbor()`)
- City borders are computed as line segments around the territory perimeter
- Border lines can be solid or dashed (for adjacent enemy cities)
- `GetTilesForGrow()` returns available tiles grouped by distance
- Pixel positions tracked for both city position and all tiles

### Buildings
- Defined by `BuildingType` configurations (passed to CityManager constructor)
- Placed on city position or city tiles (one building per tile)
- Provide yields: food, production, gold, science
- Can house citizens (Citizens property defines dwelling capacity)
- Have costs: production, purchase, upkeep, goods (Dictionary<int, int>)
- Attributes: era, invention (tech requirement), food/production levels
- Support multiple map images and one UI image
- Can be active/inactive via `IsActive` boolean

### Inhabitants
- Live in buildings (must have free dwelling space)
- Have periodic needs (checked every N rounds via interval system)
- Satisfaction level (0-100) affected by satisfied/unsatisfied needs
- Can upgrade/downgrade based on satisfaction (Type property increments/decrements)
- Each need has multiple types that can satisfy it (OR logic)
- Need satisfaction tracked per round with `LastSatisfiedRound`
- Position is immutable (set via constructor/init)

### Map Layer
- Internal map array tracks tile states:
  - `-2`: Out of bounds
  - `-1`: Unbuildable (from notPassableTiles parameter)
  - `0`: Empty
  - `1+`: City ID (only city centers, not tiles)
- City tiles are NOT stored in the map array - only tracked in city's Tiles list
- Map is flattened 1D array: index = y * columns + x

### Combat System
- `CityBase` implements `ICombatEntity` interface
- Properties: Health, MaxHealth, WeaponType, CombatStrength, RangedAttack, Range
- Seed property for random number generation in combat
- Player property identifies ownership

## Common Tasks

### Initializing CityManager
```csharp
// Prepare building definitions
List<BuildingType> buildings = new() { /* building configs */ };

// Map setup: 0 = passable, other values = obstacles
List<int> map = Enumerable.Repeat(0, rows * columns).ToList();
List<int> notPassable = new() { 1, 2, 3 }; // obstacle tile types

// Create manager
var cityManager = new CityManager(
    map, 
    rows, 
    columns, 
    notPassable, 
    buildings,
    tileWidth: 34,   // pixel width
    tileHeight: 32   // pixel height
);
```

### Creating a New City
```csharp
var city = CityFactory.CreateCityBase("CityName", playerId);
city.Position = new CubeCoordinates(q, r, s);
city.PositionPixel = new Point(x, y);
city.Health = 100;
city.MaxHealth = 100;
// Set other properties as needed (combat stats, seed, etc.)

bool success = cityManager.CreateCity(city);
if (!success) {
    // Position already occupied or unbuildable
}
```

### Expanding City Territory
```csharp
// Get available tiles within distance
var availableTiles = cityManager.GetTilesForGrow(cityId, maxDistance);
// Returns Dictionary<int, List<CubeCoordinates>> grouped by distance

// Add specific tile
bool added = cityManager.AddCityTile(cityId, tileCoordinates);

// Refresh borders after expansion (call per player)
cityManager.CreateCityBorders(playerId);
```

### Adding Buildings
```csharp
// Method 1: Using building type ID (1-indexed!)
bool built = cityManager.AddBuilding(cityId, coordinates, buildingTypeId);

// Method 2: With pre-created building instance
var building = BuildingFactory.CreateBuilding(buildingType);
bool built = cityManager.AddBuilding(cityId, coordinates, building);

// Validation checks:
// - Coordinates must be city position or city tile
// - No existing building on same tile (except city position)
// - Building instance not already in city
```

### Managing Inhabitants
```csharp
// Define needs
var needs = new List<InhabitantNeed> {
    new InhabitantNeed(new List<int> { 1, 2 }, interval: 5, satisfactionPenalty: 10)
};

// Create inhabitant
var inhabitant = new InhabitantBase(buildingPosition, needs);

// Add to city
bool added = cityManager.AddInhabitant(cityId, inhabitant);
// Validates: position is city tile, building exists, free dwellings available

// Per turn: satisfy needs
inhabitant.SatisfyNeed(buildingType, currentRound);
inhabitant.UpdateNeeds(currentRound); // End of turn - applies penalties

// Upgrade/downgrade
inhabitant.Upgrade(newNeeds);
bool downgraded = inhabitant.Downgrade(lowerNeeds);
```

### Querying Cities
```csharp
// By ID
CityBase? city = cityManager.GetCityById(cityId);

// By coordinates
CityBase? city = cityManager.GetCityByCoordinates(coords, includeTiles: true);

// By player
List<CityBase> cities = cityManager.GetCitiesOfPlayer(playerId);

// Complex queries
List<CityBase> filtered = cityManager.FindCities(
    playerId: 1, 
    buildingType: 5  // Both optional
);

// Check building existence
bool hasBuilding = cityManager.HasBuildingType(cityId, buildingType);

// Tile queries
bool isCityTile = cityManager.IsTileOfCity(cityId, coordinates);
bool isCityTile = cityManager.IsTileOfCity(coordinates, playerIds);
bool isCityCenter = cityManager.IsTileACity(coordinates);
int tileStatus = cityManager.GetTileStatus(coordinates);
```

### Border Management
```csharp
// Create borders for a player's cities
cityManager.CreateCityBorders(playerId);

// This does three things:
// 1. Computes border lines for each city
// 2. Removes duplicate borders between cities of same player
// 3. Marks overlapping borders with other players as dashed

// Access borders
var city = cityManager.GetCityById(cityId);
foreach (var border in city.Borders) {
    Point start = border.Start;
    Point end = border.End;
    bool dashed = border.Dashed;
    // Render line from start to end
}
```

### Serialization
```csharp
// JSON serialization
string json = cityManager.ToJson();
CityManager loaded = CityManager.FromJson(json);

// Custom JSON options
var options = new JsonSerializerOptions {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
string json = cityManager.ToJson(options);
CityManager loaded = CityManager.FromJson(json, options);

// Binary serialization
using var stream = new MemoryStream();
using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
{
    cityManager.Write(writer);
}

stream.Position = 0;
using (var reader = new BinaryReader(stream))
{
    CityManager loaded = CityManager.Read(reader);
}

// Format version is embedded (currently v1)
// Throws NotSupportedException for incompatible versions
```

## Testing

- Test project: `HexMapCities.Tests`
- Uses MSTest framework with `[TestClass]` and `[TestMethod]` attributes
- Test helpers in `Models/TestUtils.cs`, `Vector2.cs`, `Vector3.cs`
- Custom MSTest settings in `MSTestSettings.cs`
- Test coverage includes:
  - City creation and collision detection
  - Territory expansion and neighbor validation
  - Building placement rules
  - Inhabitant management and dwellings
  - Serialization/deserialization (dedicated test file)
  - Border generation and overlap handling
  - Utils methods

### Running Tests
```powershell
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~CityManagerTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Code Conventions

- **Namespace**: `com.hexagonsimulations.HexMapCities`
- **Internal APIs**: Marked with `[InternalsVisibleTo("HexMapCities.Tests")]`
- **Naming**: 
  - PascalCase for public APIs, types, properties, methods
  - _camelCase for private fields
  - camelCase for local variables and parameters
- **Null Safety**: Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- **JSON Attributes**: Use `[JsonPropertyName]` for serialization control
- **JSON Conventions**: 
  - camelCase property names in JSON
  - ReferenceHandler.Preserve for object references
  - DefaultIgnoreCondition.WhenWritingNull
- **Implicit Usings**: Enabled globally

## Common Pitfalls

1. **Building Type IDs are 1-indexed** in `AddBuilding(cityId, coords, buildingTypeId)` but list indices are 0-indexed
   - buildingTypeId=1 maps to _buildingDefinitions[0]
   
2. **City tiles are not stored in map layer** - only city centers are in `_map.Map`
   - Use `IsTileOfCity()` methods to check tile ownership
   
3. **Borders must be regenerated** after territory changes via `CreateCityBorders(playerId)`
   - Must be called per player, not globally
   
4. **Inhabitant needs are interval-based** - check `IsActive(currentRound)` before satisfying
   - Call `UpdateNeeds()` at end of turn to apply penalties
   
5. **Factory constructors are internal** - always use `CityFactory` and `BuildingFactory`
   - Direct instantiation will cause compilation errors
   
6. **Pixel computation has max depth** - very distant tiles from city center may fail
   - `ComputeTilePixel()` has maxDepth=5 to prevent infinite recursion
   
7. **Coordinate validation** - no automatic bounds checking on most methods
   - `GetTileStatus()` returns -2 for out-of-bounds, but other methods may not validate
   
8. **Building placement on city position** - city position can have building plus tiles can have buildings
   - Special case: buildings on city position don't count as "occupying" the tile
   
9. **Inhabitant satisfaction bounds** - clamped to 0-100 range
   - Multiple unsatisfied needs in same round stack penalties

10. **Serialization references** - objects use ReferenceHandler.Preserve
    - Same CubeCoordinates instance reused multiple times is preserved

## File Modification Guidelines

### Adding New City Properties
1. Update `CityBase` model class
2. Add JSON attribute if needed
3. Update `CityManagerState` in `CityManager.cs` serialization
4. Add tests in `CityManagerSerializationTests.cs`
5. Consider impact on border calculations if spatial property

### New Building Features
1. Update `BuildingType` record (base definition)
2. Update `BuildingBase` record if runtime state needed
3. Update `BuildingFactory.CreateBuilding()` to copy new properties
4. Update building-related tests
5. Consider validation in `CityManager.AddBuilding()`

### Map Logic Changes
1. Modify `CityManager` methods
2. Update `MapData` if storage structure changes
3. Update tile status enum if needed
4. Add/update tests in `CityManagerTests.cs`
5. Document any API changes

### Border Calculation Changes
1. Modify methods in `Utils` class (internal)
2. Test with various city configurations
3. Consider performance for large city counts
4. Update border-related tests

### Coordinate/Pixel Conversions
1. Rely on HexMapBase library's `CubeCoordinates` methods
2. For pixel logic, modify `Utils.ComputeTilePixel()` carefully
3. Consider PRECISION constant (1.0001f) for float comparisons
4. Test with various tile sizes

### Adding New Manager Methods
1. Add public method to `CityManager`
2. Document with XML comments (summary, params, returns)
3. Consider early exit patterns (null checks, validation)
4. Return appropriate types (bool for success, null for not found)
5. Add comprehensive tests

## Dependencies & External Types

### From HexMapBase (v0.5.0)
- `CubeCoordinates`: Core hex coordinate type
  - Methods: `Distance(CubeCoordinates a, CubeCoordinates b)`, `Neighbors()`, `ToOffset()`
  - Operators: `==`, `+`, `-`
  - Properties: `q`, `r`, `s`
- `OffsetCoordinates`: 2D coordinates for array indexing
  - Properties: `x`, `y`
- `ICombatEntity`: Interface implemented by `CityBase`
  - Properties: Id, Player, Health, MaxHealth

### System Dependencies
- System.Text.Json for serialization
- System.IO for binary serialization
- Standard .NET collections (Dictionary, List, HashSet, Queue)

## Version History Notes

- **Current**: 0.5.4 (November 2025)
- **Recent releases**: 0.3.x → 0.4.x → 0.5.x
- **Target framework**: Upgraded from .NET 8.0 to .NET 10.0
- Multiple .nuspec files in `obj/Release/` suggest frequent releases
- Maintained compatibility with HexMapBase 0.5.0

### Migration Notes
If upgrading from earlier versions:
- Check for breaking changes in HexMapBase dependency
- Serialization format is versioned (currently v1)
- Internal constructor pattern may affect custom city/building creation
- .NET 10.0 required for latest version

## Performance Considerations

1. **City lookup**: O(1) by ID via Dictionary
2. **Coordinate lookup**: O(n) requires iterating all cities
3. **Territory expansion**: `GetTilesForGrow()` uses BFS (breadth-first search)
4. **Border calculation**: O(n*m) where n=cities, m=borders per city
5. **Serialization**: JSON is text-based (slower but readable), binary is faster

### Optimization Tips
- Cache frequently accessed cities rather than looking up by ID repeatedly
- Batch border updates - only call `CreateCityBorders()` when necessary
- Use `FindCities()` with specific filters instead of `GetCitiesOfPlayer()` + filtering
- Consider external caching for coordinate-to-city lookups if performance critical

## Project Structure

```
HexMapCities/
├── CityManager.cs           # Main API
├── HexMapCities.csproj      # Project configuration
├── Enums/
│   └── TileType.cs          # Internal tile states
└── Models/
    ├── BuildingBase.cs      # Building instance
    ├── BuildingFactory.cs   # Building creation
    ├── BuildingType.cs      # Building definition
    ├── CityBase.cs          # City entity
    ├── CityFactory.cs       # City creation
    ├── InhabitantBase.cs    # Citizen entity
    ├── InhabitantNeed.cs    # Need system
    ├── Line.cs              # Border line segment
    ├── MapData.cs           # Internal map storage
    ├── Point.cs             # Pixel coordinates
    └── Utils.cs             # Internal helpers

HexMapCities.Tests/
├── CityManagerTests.cs                  # Core functionality tests
├── CityManagerSerializationTests.cs     # Serialization tests
├── UtilsTests.cs                        # Utility method tests
├── MSTestSettings.cs                    # Test configuration
└── Models/
    ├── TestUtils.cs         # Test helpers
    ├── Vector2.cs           # Test data structures
    └── Vector3.cs           # Test data structures
```

## Usage Examples

### Complete City Setup Example
```csharp
// 1. Define buildings
var buildingTypes = new List<BuildingType> {
    new BuildingType {
        Name = "Dwelling",
        Type = 1,
        Citizens = 2,
        Food = 2,
        ProductionCost = 10,
        // ... other properties
    }
};

// 2. Initialize manager
var map = Enumerable.Repeat(0, 100).ToList();
var cityManager = new CityManager(map, 10, 10, new(), buildingTypes, 34, 32);

// 3. Create city
var city = CityFactory.CreateCityBase("Rome", playerId: 1);
city.Position = new CubeCoordinates(0, 0, 0);
city.PositionPixel = new Point(100, 100);
city.Health = 100;
city.MaxHealth = 100;
cityManager.CreateCity(city);

// 4. Expand territory
var growth = cityManager.GetTilesForGrow(city.Id, 2);
foreach (var tile in growth[1]) { // Distance 1 tiles
    cityManager.AddCityTile(city.Id, tile);
}

// 5. Build structures
cityManager.AddBuilding(city.Id, city.Position, buildingTypeId: 1);

// 6. Add inhabitants
var needs = new List<InhabitantNeed> {
    new(new List<int> { 1 }, interval: 3, satisfactionPenalty: 5)
};
var inhabitant = new InhabitantBase(city.Position, needs);
cityManager.AddInhabitant(city.Id, inhabitant);

// 7. Update borders
cityManager.CreateCityBorders(playerId: 1);

// 8. Save state
string savedGame = cityManager.ToJson();
```

## Troubleshooting

### Common Error Messages

**"Position already occupied or not possible"**
- City creation failed due to tile collision
- Check map for UNBUILDABLE tiles or existing cities

**"Failed to deserialize CityManagerState"**
- Corrupted JSON or version mismatch
- Verify JSON structure and format version

**"Unsupported CityManager binary format version X"**
- Binary file from incompatible version
- Current version only supports v1

### Debugging Tips

1. **City not appearing**: Check if `CreateCity()` returned true
2. **Can't expand territory**: Verify tiles are neighbors via `IsCityNeighbor()`
3. **Building won't place**: Check for existing buildings on tile
4. **Inhabitant rejected**: Verify building has free dwellings
5. **Borders missing**: Call `CreateCityBorders()` after territory changes
6. **Serialization fails**: Check for circular references or unsupported types

## Future Development Guidelines

When extending this library:

1. **Maintain backward compatibility** in serialization
2. **Use factory pattern** for new entity types
3. **Add XML documentation** to all public APIs
4. **Include unit tests** for new features
5. **Update version number** appropriately (semantic versioning)
6. **Consider performance** for operations on many cities
7. **Document breaking changes** clearly
8. **Test serialization** of new properties
9. **Keep internal APIs internal** unless necessary
10. **Follow existing naming conventions**

## Contact & Support

- **Repository**: https://github.com/Ziagl/hex-map-cities
- **Company**: Hexagon Simulations
- **Website**: https://hexagon-simulations.com/
- **Author**: Werner Ziegelwanger

For bug reports and feature requests, use GitHub Issues.
