namespace VertigoCase.Core
{
    /// <summary>
    /// The behaviour contract for one zone type (Strategy pattern). Each concrete strategy decides
    /// whether the zone carries a bomb and how a slice's base reward scales with the zone number.
    /// Callers depend on this interface, not on the concrete classes, so adding a new zone type does
    /// not force existing code to change (Open/Closed).
    /// </summary>
    public interface IZoneStrategy
    {
        bool HasBomb { get; }                        // normal -> true, safe/super -> false
        int ScaleReward(int baseAmount, int zone);   // reward grows as the zone number grows
    }
}