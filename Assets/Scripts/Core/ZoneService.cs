namespace VertigoCase.Core
{
    /// <summary>
    /// Tracks which zone the player is on and decides that zone's type from the case rules:
    /// every 5th zone is Safe (silver), every 30th is Super (golden), everything else is Normal
    /// (bronze). Pure C# (no MonoBehaviour) so it can be unit-tested without a scene.
    /// </summary>
    public class ZoneService
    {
        // The zone the player is currently on. Read-only from outside; only Advance() changes it.
        public int CurrentZone { get; private set; } = 1;

        // Maps a zone number to its type. Super is checked FIRST: every 30th zone is also a multiple
        // of 5, so the order is what makes 30/60/... resolve to Super instead of Safe.
        public ZoneType TypeOf(int zone)
        {
            if (zone % 30 == 0) return ZoneType.Super; // every 30th -> golden
            if (zone % 5  == 0) return ZoneType.Safe;  // every 5th  -> silver
            return ZoneType.Normal;                    // otherwise  -> bronze
        }

        // Convenience accessor: the type of the zone we are on right now.
        public ZoneType CurrentType => TypeOf(CurrentZone);

        // Move to the next zone after a successful (non-bomb) spin.
        public void Advance() => CurrentZone++;
    }
}
