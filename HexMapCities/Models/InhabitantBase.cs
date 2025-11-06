using System.Text.Json.Serialization;
using com.hexagonsimulations.HexMapBase.Models;

namespace com.hexagonsimulations.HexMapCities.Models;

public class InhabitantBase
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("position")]
    public CubeCoordinates Position { get; init; } // the position of the building this inhabitant lives

    [JsonPropertyName("needs")]
    public List<InhabitantNeed> Needs { get; set; }

    [JsonPropertyName("satisfaction")]
    public int Satisfaction { get; set; }

    [JsonConstructor]
    public InhabitantBase()
    {
        Needs = new List<InhabitantNeed>();
    }

    public InhabitantBase(CubeCoordinates position, List<InhabitantNeed> needs)
    {
        Type = 1;
        Satisfaction = 100;
        Position = position;
        Needs = needs;
    }

    public void Upgrade(List<InhabitantNeed> needs)
    {
        Type++;
        Satisfaction = 100; // Reset satisfaction on upgrade
        Needs = needs;
    }

    public bool Downgrade(List<InhabitantNeed> needs)
    {
        if (Type > 1)
        {
            Type--;
            Satisfaction = 100; // Reset satisfaction on downgrade
            Needs = needs;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Satisfys a specific need by type
    /// </summary>
    /// <param name="type">Type of need.</param>
    /// <param name="currentRound">Current round number.</param>
    public void SatisfyNeed(int type, int currentRound)
    {
        var need = Needs.FirstOrDefault(n => n.Types.Contains(type) && n.IsActive(currentRound));
        if (need != null)
        {
            need.Satisfy(currentRound);
            Satisfaction += need.SatisfactionPenalty;
            if (Satisfaction > 100)
            {
                Satisfaction = 100;
            }
        }
    }

    /// <summary>
    /// This should be called at the end of the turn after satisfying needs
    /// </summary>
    /// <param name="currentRound">Current round number.</param>
    public void UpdateNeeds(int currentRound)
    {
        foreach (var need in Needs)
        {
            if (need.IsActive(currentRound))
            {
                // Need was not satisfied this round
                Satisfaction -= need.SatisfactionPenalty;
                if (Satisfaction < 0)
                {
                    Satisfaction = 0;
                }
            }
        }
    }
}
