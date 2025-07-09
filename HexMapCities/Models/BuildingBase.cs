using com.hexagonsimulations.HexMapBase.Models;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingBase : BuildingType
{
    // base
    public bool IsActive = true; // is this building active?
    // position
    public CubeCoordinates Position; // its position on the map
}