﻿namespace com.hexagonsimulations.HexMapCities.Models;

public record BuildingType
{
    // base
    public string Name = string.Empty; // the name of this building type
    public string Description = string.Empty; // short description for what this building is used
    public int Type; // the type of this unit (value of an enum?)
    public int Era; // min era for this building
    public int Invention; // invention needed to build this building
    // economy
    public Tuple<int, int> Production = new(0,0); // goods produced by this building (goods id, amount)
    public List<Tuple<int, int>> Goods = new(); // goods needed to produce this building (goods id, amount)
    public int ProductionCost; // amount of production needed to build this building
    public int PurchaseCost; // amount of gold needed to purchase this building
}