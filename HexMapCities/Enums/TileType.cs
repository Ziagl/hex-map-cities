using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HexMapCities.Tests")]

namespace HexMapCities.Enums;

internal enum TileType
{
    UNBUILDABLE = -1,
    EMPTY = 0,
}
