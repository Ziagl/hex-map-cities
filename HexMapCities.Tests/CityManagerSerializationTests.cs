using com.hexagonsimulations.HexMapCities.Enums;
using com.hexagonsimulations.HexMapCities.Models;
using com.hexagonsimulations.HexMapCities.Tests.Models;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.hexagonsimulations.HexMapCities.Tests;

[TestClass]
public sealed class CityManagerSerializationTests
{
    private readonly string TempDir = @"C:\Temp\";
    private readonly bool DumpToDisk = false; // set to true to dump serialized data to disk for inspection

    private readonly List<BuildingType> _buildingDefinitions = new();

    public CityManagerSerializationTests()
    {
        _buildingDefinitions.Add(new BuildingType() { });
    }

    [TestMethod]
    public void CityManager_Json()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        exampleMap[0] = (int)TileType.UNBUILDABLE;
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>() { (int)TileType.UNBUILDABLE }, new List<BuildingType>());
        var city = TestUtils.CreateExampleCity1();
        var city2 = TestUtils.CreateExampleCity2();
        cityManager.CreateCity(city);
        cityManager.CreateCity(city2);

        var json = cityManager.ToJson();
        Assert.IsFalse(string.IsNullOrWhiteSpace(json), "JSON should not be empty.");

        if (DumpToDisk)
        {
            File.WriteAllText($"{TempDir}CityManager.json", json);
        }

        var roundTripped = CityManager.FromJson(json);
        Assert.IsNotNull(roundTripped, "Deserialized CityManager should not be null.");

        AssertCityManagerEqual(cityManager, roundTripped);
    }

    [TestMethod]
    public void CityManager_Binary()
    {
        var exampleMap = Enumerable.Repeat(0, 16).ToList();
        exampleMap[0] = (int)TileType.UNBUILDABLE;
        var cityManager = new CityManager(exampleMap, 4, 4, new List<int>() { (int)TileType.UNBUILDABLE }, new List<BuildingType>());
        var city = TestUtils.CreateExampleCity1();
        var city2 = TestUtils.CreateExampleCity2();
        cityManager.CreateCity(city);
        cityManager.CreateCity(city2);

        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            cityManager.Write(writer);
        }

        if (DumpToDisk)
        {
            File.WriteAllBytes($"{TempDir}CityManager.bin", ms.ToArray());
        }

        ms.Position = 0;
        CityManager roundTripped;
        using (var reader = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            roundTripped = CityManager.Read(reader);
        }

        Assert.IsNotNull(roundTripped, "Binary deserialized CityManager should not be null.");
        AssertCityManagerEqual(cityManager, roundTripped);
    }

    private static void AssertCityManagerEqual(CityManager expected, CityManager actual)
    {
        var differences = new List<string>();
        var visited = new HashSet<string>(); // reference pair signatures to prevent cycles
        DeepCompare(expected, actual, "CityManager", differences, visited);

        if (differences.Count > 0)
        {
            var message = "CityManager objects differ (" + differences.Count + " difference(s)):\n"
                          + string.Join("\n", differences.Take(50));
            Assert.Fail(message);
        }
    }

    private static readonly Type[] _terminalTypes =
    {
        typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset),
        typeof(TimeSpan), typeof(Guid)
    };

    private static bool IsTerminal(Type t) =>
        t.IsPrimitive || t.IsEnum || _terminalTypes.Contains(t);

    private static void DeepCompare(object? expected,
                                    object? actual,
                                    string path,
                                    List<string> diffs,
                                    HashSet<string> visited)
    {
        if (ReferenceEquals(expected, actual)) return;

        if (expected is null || actual is null)
        {
            diffs.Add($"{path}: One is null, the other is not (expected {(expected is null ? "null" : "value")}, actual {(actual is null ? "null" : "value")}).");
            return;
        }

        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        if (expectedType != actualType)
        {
            diffs.Add($"{path}: Type mismatch (expected {expectedType.FullName}, actual {actualType.FullName}).");
            return;
        }

        if (IsTerminal(expectedType))
        {
            if (!Equals(expected, actual))
                diffs.Add($"{path}: Value mismatch (expected '{expected}', actual '{actual}').");
            return;
        }

        // Prevent infinite recursion (cycles)
        if (!expectedType.IsValueType)
        {
            var sig = $"{RuntimeHelpers.GetHashCode(expected)}|{RuntimeHelpers.GetHashCode(actual)}";
            if (visited.Contains(sig)) return;
            visited.Add(sig);
        }

        // IDictionary
        if (expected is IDictionary expectedDict && actual is IDictionary actualDict)
        {
            if (expectedDict.Count != actualDict.Count)
                diffs.Add($"{path}: Dictionary count mismatch (expected {expectedDict.Count}, actual {actualDict.Count}).");

            var expectedKeys = expectedDict.Keys.Cast<object?>().ToList();
            foreach (var k in expectedKeys)
            {
                var subPath = $"{path}[Key={k}]";
                if (!actualDict.Contains(k))
                {
                    diffs.Add($"{subPath}: Missing key in actual.");
                    continue;
                }
                DeepCompare(expectedDict[k], actualDict[k], subPath, diffs, visited);
            }

            // Extra keys in actual
            foreach (var k in actualDict.Keys.Cast<object?>())
            {
                if (!expectedDict.Contains(k))
                    diffs.Add($"{path}[Key={k}]: Extra key in actual not present in expected.");
            }
            return;
        }

        // IEnumerable (but not string)
        if (expected is IEnumerable expectedEnum && actual is IEnumerable actualEnum && expected is not string)
        {
            var expectedList = expectedEnum.Cast<object?>().ToList();
            var actualList = actualEnum.Cast<object?>().ToList();

            if (expectedList.Count != actualList.Count)
                diffs.Add($"{path}: Collection count mismatch (expected {expectedList.Count}, actual {actualList.Count}).");

            var count = Math.Min(expectedList.Count, actualList.Count);
            for (int i = 0; i < count; i++)
            {
                DeepCompare(expectedList[i], actualList[i], $"{path}[{i}]", diffs, visited);
            }
            return;
        }

        // Object: compare public readable properties (skip indexers, delegates)
        var props = expectedType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();

        foreach (var p in props)
        {
            object? expVal = null;
            object? actVal = null;
            bool gotExp = true, gotAct = true;
            try { expVal = p.GetValue(expected); } catch { gotExp = false; }
            try { actVal = p.GetValue(actual); } catch { gotAct = false; }

            if (!gotExp || !gotAct)
            {
                if (gotExp != gotAct)
                    diffs.Add($"{path}.{p.Name}: Unable to read property consistently (expected readable={gotExp}, actual readable={gotAct}).");
                continue;
            }

            if (typeof(Delegate).IsAssignableFrom(p.PropertyType)) continue;

            DeepCompare(expVal, actVal, $"{path}.{p.Name}", diffs, visited);
        }

        // Optionally include public fields (records with public fields, etc.)
        var fields = expectedType
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => !f.IsInitOnly && !typeof(Delegate).IsAssignableFrom(f.FieldType));

        foreach (var f in fields)
        {
            var expVal = f.GetValue(expected);
            var actVal = f.GetValue(actual);
            DeepCompare(expVal, actVal, $"{path}.{f.Name}", diffs, visited);
        }
    }
}