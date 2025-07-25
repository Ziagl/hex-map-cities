﻿using com.hexagonsimulations.HexMapBase.Interfaces;
using com.hexagonsimulations.HexMapBase.Models;
using HexMapCities.Models;

namespace com.hexagonsimulations.HexMapCities.Models;

public class CityBase : ICombatEntity
{
    // base entity
    public int Id { get; set; }
    public int Player { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    // base city
    public string Name { get; set; } = string.Empty;
    // combat
    public int WeaponType { get; set; } // type of weapon/combat of this unit (infantry, cavalry, ...)
    public int CombatStrength { get; set; } // attack points (damage in fight)
    public int RangedAttack { get; set; } // ranged attack points (damage of airstrike)
    public int Range { get; set; } // attack range (how far can this unit attack)
    // random number
    public int Seed { get; set; } // random number seed (for random number generation)
    // position
    public CubeCoordinates Position { get; set; }
    public List<CubeCoordinates> Tiles { get; set; } = new();
    // UI
    public Point PositionPixel { get; set; } = new();      // absolute position in pixels
    public List<Point> TilesPixel { get; set; } = new();   // absolute positions in pixels
    public List<Line> Borders { get; set; } = new();       // border lines in absolute pixels
    // buildings
    public List<BuildingBase> Buildings { get; set; } = new();
    // inhabitants
    public List<InhabitantBase> Inhabitants { get; set; } = new();
    // properties
    public Dictionary<string, object> Properties { get; set; } = new();
}
