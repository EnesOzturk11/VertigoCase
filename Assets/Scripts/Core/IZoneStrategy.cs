namespace VertigoCase.Core
{
    /// <summary>
    /// Rules exposed by one zone type. Reward values and bomb presence remain wheel content, so a
    /// value displayed on a slice is never transformed again after the player wins it.
    /// </summary>
    public interface IZoneStrategy
    {
        ZoneType Type { get; }
        bool CanLeave { get; }
    }
}
