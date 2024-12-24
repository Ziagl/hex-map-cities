using com.hexagonsimulations.HexMapBase.Geometry.Hex;

namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingBase : BuildingType
{
    // base
    
    // position
    public CubeCoordinates Position; // its position on the map
}