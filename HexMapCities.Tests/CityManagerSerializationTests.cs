using com.hexagonsimulations.HexMapCities.Enums;
using com.hexagonsimulations.HexMapCities.Models;
using com.hexagonsimulations.HexMapCities.Tests.Models;
using System.Reflection;

namespace com.hexagonsimulations.HexMapCities.Tests;

[TestClass]
public sealed class CityManagerSerializationTests
{
    private readonly string TempDir = @"C:\Temp\";

    private readonly List<BuildingType> _buildingDefinitions = new();

    public CityManagerSerializationTests()
    {
        _buildingDefinitions.Add(new BuildingType() { });
    }

    [TestMethod]
    public void CityManager_Json()
    {
        var cityManager = CreateTestData();

        var json = cityManager.ToJson();
        Assert.IsFalse(string.IsNullOrWhiteSpace(json), "JSON should not be empty.");

#if DEBUG
        File.WriteAllText($"{TempDir}CityManager.json", json);
#endif

        var roundTripped = CityManager.FromJson(json);
        Assert.IsNotNull(roundTripped, "Deserialized CityManager should not be null.");

        AssertCityManagerEqual(cityManager, roundTripped);
    }

    [TestMethod]
    public void CityManager_Binary()
    {
        var cityManager = CreateTestData();

        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            cityManager.Write(writer);
        }

#if DEBUG
        File.WriteAllBytes($"{TempDir}CityManager.bin", ms.ToArray());
#endif

        ms.Position = 0;
        CityManager roundTripped;
        using (var reader = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            roundTripped = CityManager.Read(reader);
        }

        Assert.IsNotNull(roundTripped, "Binary deserialized CityManager should not be null.");
        AssertCityManagerEqual(cityManager, roundTripped);
    }

    private CityManager CreateTestData()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        exampleMap[9] = (int)TileType.UNBUILDABLE;
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>() { (int)TileType.UNBUILDABLE }, TestUtils.CreateBuildingTypes());
        var city = TestUtils.CreateExampleCity1();
        var city2 = TestUtils.CreateExampleCity2();
        cityManager.CreateCity(city);
        var newTilePosition = new HexMapBase.Models.CubeCoordinates(-1, 2, -1);
        cityManager.AddCityTile(city.Id, newTilePosition);
        cityManager.AddBuilding(city.Id, newTilePosition, 0);
        cityManager.CreateCity(city2);

        return cityManager;
    }

    private static void AssertCityManagerEqual(CityManager expected, CityManager actual)
    {
        // Use reflection to access private fields
        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        // Compare _cityStore
         var expectedCityStore = (Dictionary<int, CityBase>)expectedType
            .GetField("_cityStore", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(expected)!;
        var actualCityStore = (Dictionary<int, CityBase>)actualType
            .GetField("_cityStore", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(actual)!;

        Assert.AreEqual(expectedCityStore.Count, actualCityStore.Count, "CityStore counts do not match.");
        foreach (var key in expectedCityStore.Keys)
        {
            Assert.IsTrue(actualCityStore.ContainsKey(key), $"CityStore key {key} is missing in actual.");
            Assert.AreEqual(expectedCityStore[key].Id, actualCityStore[key].Id, $"CityStore city ID mismatch for key {key}.");
        }

        // Compare _lastCityStoreId
        var expectedLastCityStoreId = (int)expectedType
            .GetField("_lastCityStoreId", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(expected)!;
        var actualLastCityStoreId = (int)actualType
            .GetField("_lastCityStoreId", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(actual)!;

        Assert.AreEqual(expectedLastCityStoreId, actualLastCityStoreId, "LastCityStoreId does not match.");

        // Compare _map
        var expectedMap = (MapData)expectedType
            .GetField("_map", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(expected)!;
        var actualMap = (MapData)actualType
            .GetField("_map", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(actual)!;

        Assert.AreEqual(expectedMap.Rows, actualMap.Rows, "Map rows do not match.");
        Assert.AreEqual(expectedMap.Columns, actualMap.Columns, "Map columns do not match.");
        CollectionAssert.AreEqual(expectedMap.Map, actualMap.Map, "Map data does not match.");

        // Compare _buildingDefinitions
        var expectedBuildingDefinitions = (List<BuildingType>)expectedType
            .GetField("_buildingDefinitions", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(expected)!;
        var actualBuildingDefinitions = (List<BuildingType>)actualType
            .GetField("_buildingDefinitions", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(actual)!;

        Assert.AreEqual(expectedBuildingDefinitions.Count, actualBuildingDefinitions.Count, "BuildingDefinitions count does not match.");
        for (int i = 0; i < expectedBuildingDefinitions.Count; i++)
        {
            Assert.AreEqual(expectedBuildingDefinitions[i].Name, actualBuildingDefinitions[i].Name, $"BuildingDefinition mismatch at index {i}.");
        }
    }
}