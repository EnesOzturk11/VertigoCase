using System;
using VertigoCase.Config;

namespace VertigoCase.Core
{
    /// <summary>
    /// Tracks the current zone while delegating zone-type rules to an injected resolver. It owns
    /// progression only; occurrence rules remain configurable outside this class.
    /// </summary>
    public sealed class ZoneService : IZoneProgression
    {
        private readonly IZoneTypeResolver zoneTypes;

        // The zone the player is currently on. Read-only from outside; only Advance() changes it.
        public int CurrentZone { get; private set; } = GameConstants.Zones.FirstZone;

        public ZoneService(IZoneTypeResolver zoneTypes)
        {
            this.zoneTypes = zoneTypes ?? throw new ArgumentNullException(nameof(zoneTypes));
        }

        public ZoneType TypeOf(int zone) => zoneTypes.ResolveType(zone);

        // Convenience accessor: the type of the zone we are on right now.
        public ZoneType CurrentType => TypeOf(CurrentZone);

        // Move to the next zone after a successful (non-bomb) spin.
        public void Advance() => CurrentZone = checked(CurrentZone + 1);

        // Start a new run without replacing the service instance and its dependencies.
        public void Reset() => CurrentZone = GameConstants.Zones.FirstZone;
    }
}
