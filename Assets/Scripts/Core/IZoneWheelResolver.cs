using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>Resolves visual wheel data without exposing catalog storage.</summary>
    public interface IZoneWheelResolver
    {
        WheelData WheelFor(ZoneType type);
    }
}
