namespace com.hexagonsimulations.HexMapCities.Models;

public static class CityFactory
{
    public static CityBase CreateCityBase(string name, int player)
        =>  new CityBase
        {
            Name = name,
            Player = player
        };
}
