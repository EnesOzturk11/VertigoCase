namespace VertigoCase.Core
{
    /// <summary>Minimal zone progression contract required by a game session.</summary>
    public interface IZoneProgression
    {
        int CurrentZone { get; }
        ZoneType CurrentType { get; }
        ZoneType TypeOf(int zone);
        void Advance();
        void Reset();
    }
}
