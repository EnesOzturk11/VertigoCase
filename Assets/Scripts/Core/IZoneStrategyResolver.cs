namespace VertigoCase.Core
{
    /// <summary>
    /// Resolves a reward strategy without exposing its registration mechanism to gameplay code.
    /// </summary>
    public interface IZoneStrategyResolver
    {
        IZoneStrategy Resolve(ZoneType type);
    }
}
