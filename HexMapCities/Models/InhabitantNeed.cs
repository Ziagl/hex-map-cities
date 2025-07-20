namespace HexMapCities.Models;

public class InhabitantNeed
{
    public int Type { get; init; }
    public int Interval { get; init; } // Every how many rounds the need becomes active
    public int LastSatisfiedRound { get; private set; }
    public int SatisfactionPenalty { get; init; }

    public InhabitantNeed(int type, int interval, int satisfactionPenalty = 0)
    {
        Type =  type;
        Interval = interval;
        SatisfactionPenalty = satisfactionPenalty;
        LastSatisfiedRound = 0;
    }

    /// <summary>
    /// This method checks if the need is active based on the current round.
    /// </summary>
    /// <param name="currentRound">Current round number.</param>
    /// <returns>true if this need should be satisfyed next or false if not needed</returns>
    public bool IsActive(int currentRound)
    {
        return (currentRound - LastSatisfiedRound) >= Interval;
    }

    /// <summary>
    /// Satisfys this need for the current round.
    /// </summary>
    /// <param name="currentRound">Round number this need was satisfied last.</param>
    public void Satisfy(int currentRound)
    {
        LastSatisfiedRound = currentRound;
    }

}
