namespace VertigoCase.Core
{
    /// <summary>
    /// Reward policy for one zone type (Strategy pattern). The contract contains only the operation
    /// its client needs; bomb presence remains wheel content, not a strategy responsibility.
    /// </summary>
    public interface IZoneStrategy
    {
        ZoneType Type { get; }
        bool CanLeave { get; }
        int ScaleReward(int baseAmount, int zone);
    }
}
