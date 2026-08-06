using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>
    /// Configurable policy asset for a zone family. It owns zone routing and leave rules while the
    /// selected wheel remains the single source of truth for reward amounts.
    /// </summary>
    public abstract class ZonePolicy : ScriptableObject, IZoneStrategy
    {
        [SerializeField] private ZoneType type;
        [SerializeField, Min(GameConstants.Zones.MinimumOccurrenceInterval)]
        private int occurrenceInterval = GameConstants.Zones.DefaultOccurrenceInterval;
        [SerializeField] private WheelData wheel;
        [SerializeField] private bool canLeave;

        public ZoneType Type => type;
        public int OccurrenceInterval => occurrenceInterval;
        public WheelData Wheel => wheel;
        public bool CanLeave => canLeave;

        public bool AppliesTo(int zone) =>
            zone >= GameConstants.Zones.FirstZone &&
            occurrenceInterval >= GameConstants.Zones.MinimumOccurrenceInterval &&
            zone % occurrenceInterval == 0;

    }
}
