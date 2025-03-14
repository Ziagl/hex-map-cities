using com.hexagonsimulations.HexMapBase.Models;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingBase : BuildingType
{
    // base
    
    // position
    public CubeCoordinates Position; // its position on the map
}