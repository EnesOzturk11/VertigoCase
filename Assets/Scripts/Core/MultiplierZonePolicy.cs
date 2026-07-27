using UnityEngine;
using VertigoCase.Config;

namespace VertigoCase.Core
{
    /// <summary>
    /// Configurable zone policy whose reward is multiplied by the current zone and an asset value.
    /// Other reward algorithms can extend <see cref="ZonePolicy"/> without changing the catalog.
    /// </summary>
    [CreateAssetMenu(fileName = "ZonePolicy_", menuName = "Vertigo/Zone Policy/Multiplier")]
    public sealed class MultiplierZonePolicy : ZonePolicy
    {
        [SerializeField, Min(GameConstants.Rewards.MinimumZoneMultiplier)]
        private int rewardMultiplier = GameConstants.Rewards.DefaultZoneMultiplier;

        public override int ScaleReward(int baseAmount, int zone) =>
            baseAmount * zone * rewardMultiplier;
    }
}
