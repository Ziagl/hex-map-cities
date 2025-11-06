using com.hexagonsimulations.HexMapBase.Models;
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
        var buildingTypes = TestUtils.CreateBuildingTypes();
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>() { (int)TileType.UNBUILDABLE }, buildingTypes, 50, 50);
        var city = TestUtils.CreateExampleCity1();
        var city2 = TestUtils.CreateExampleCity2();
        bool success = cityManager.CreateCity(city);
        Assert.IsTrue(success);
        var newTilePosition = new CubeCoordinates(-1, 2, -1);
        success = cityManager.AddCityTile(city.Id, newTilePosition);
        Assert.IsTrue(success);
        success = cityManager.AddBuilding(city.Id, newTilePosition, 3 /*House*/);
        Assert.IsTrue(success);
        success = cityManager.AddInhabitant(city.Id, new InhabitantBase(newTilePosition, TestUtils.CreateInhabitantNeeds()));
        Assert.IsTrue(success);
        cityManager.CreateCity(city2);
        cityManager.CreateCityBorders(city.Player);

        return cityManager;
    }

    private static void AssertCityBaseEqual(CityBase expected, CityBase actual)
    {
        Assert.AreEqual(expected.Id, actual.Id, "City Id mismatch");
        Assert.AreEqual(expected.Player, actual.Player, "City Player mismatch");
        Assert.AreEqual(expected.Health, actual.Health, "City Health mismatch");
        Assert.AreEqual(expected.MaxHealth, actual.MaxHealth, "City MaxHealth mismatch");
        Assert.AreEqual(expected.Name, actual.Name, "City Name mismatch");
        Assert.AreEqual(expected.WeaponType, actual.WeaponType, "City WeaponType mismatch");
        Assert.AreEqual(expected.CombatStrength, actual.CombatStrength, "City CombatStrength mismatch");
        Assert.AreEqual(expected.RangedAttack, actual.RangedAttack, "City RangedAttack mismatch");
        Assert.AreEqual(expected.Range, actual.Range, "City Range mismatch");
        Assert.AreEqual(expected.Seed, actual.Seed, "City Seed mismatch");

        Assert.AreEqual(expected.Position, actual.Position, "City Position mismatch");
        CollectionAssert.AreEqual(expected.Tiles, actual.Tiles, "City Tiles mismatch");
        Assert.AreEqual(expected.PositionPixel, actual.PositionPixel, "City PositionPixel mismatch");
        CollectionAssert.AreEqual(expected.TilesPixel, actual.TilesPixel, "City TilesPixel mismatch");
        CollectionAssert.AreEqual(expected.Borders, actual.Borders, "City Borders mismatch");

        Assert.AreEqual(expected.Buildings.Count, actual.Buildings.Count, "City Buildings count mismatch");
        for (int i = 0; i < expected.Buildings.Count; i++)
        {
            var expectedBuilding = expected.Buildings[i];
            var actualBuilding = actual.Buildings[i];
            Assert.AreEqual(expectedBuilding.Type, actualBuilding.Type, $"Building Type mismatch at index {i}");
            Assert.AreEqual(expectedBuilding.IsActive, actualBuilding.IsActive, $"Building IsActive mismatch at index {i}");
            Assert.AreEqual(expectedBuilding.Position, actualBuilding.Position, $"Building Position mismatch at index {i}");
        }

        Assert.AreEqual(expected.Inhabitants.Count, actual.Inhabitants.Count, "City Inhabitants count mismatch");
        for (int i = 0; i < expected.Inhabitants.Count; i++)
        {
            var expectedInhabitant = expected.Inhabitants[i];
            var actualInhabitant = actual.Inhabitants[i];
            Assert.AreEqual(expectedInhabitant.Type, actualInhabitant.Type, $"Inhabitant Type mismatch at index {i}");
            Assert.AreEqual(expectedInhabitant.Position, actualInhabitant.Position, $"Inhabitant Position mismatch at index {i}");
            Assert.AreEqual(expectedInhabitant.Satisfaction, actualInhabitant.Satisfaction, $"Inhabitant Satisfaction mismatch at index {i}");
            Assert.AreEqual(expectedInhabitant.Needs.Count, actualInhabitant.Needs.Count, "Needs count mismatch");
            for (int j = 0; j < expectedInhabitant.Needs.Count; j++)
            {
                Assert.AreEqual(expectedInhabitant.Needs[j].LastSatisfiedRound, actualInhabitant.Needs[j].LastSatisfiedRound, $"Need LastSatisfiedRound mismatch at index {j}");
                Assert.AreEqual(expectedInhabitant.Needs[j].SatisfactionPenalty, actualInhabitant.Needs[j].SatisfactionPenalty, $"Need SatisfactionPenalty mismatch at index {j}");
                Assert.AreEqual(expectedInhabitant.Needs[j].Interval, actualInhabitant.Needs[j].Interval, $"Need Interval mismatch at index {j}");
                CollectionAssert.AreEqual(expectedInhabitant.Needs[j].Types, actualInhabitant.Needs[j].Types, $"Need Types mismatch at index {j}");
            }
        }

        CollectionAssert.AreEqual(
            expected.Properties.OrderBy(x => x.Key).ToList(),
            actual.Properties.OrderBy(x => x.Key).ToList(),
            "City Properties mismatch"
        );
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
            AssertCityBaseEqual(expectedCityStore[key], actualCityStore[key]);
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
