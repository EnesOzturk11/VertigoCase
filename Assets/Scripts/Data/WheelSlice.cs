using System;
using UnityEngine;

namespace VertigoCase.Data
{
    /// <summary>
    /// A single slice on the wheel. This is NOT a ScriptableObject; it lives as a list
    /// element inside <see cref="WheelData"/>. Every slice (the bomb included) points to a
    /// RewardData, so the slice itself stays tiny.
    /// </summary>
    [Serializable] // makes Unity show and edit this class in the Inspector
    public class WheelSlice
    {
        [Tooltip("Reward for this slice. The bomb is a RewardData whose type is Bomb.")]
        public RewardData reward;

        [Tooltip("Amount multiplier applied to the reward's base amount.")]
        public int multiplier = 1;

        [Tooltip("Selection weight. Higher = comes up more often. Rare rewards / the bomb get a lower weight.")]
        public int weight = 1;

        // Derived from the reward type, so there is a single source of truth (no extra bool to keep in sync).
        public bool IsBomb => reward != null && reward.type == RewardType.Bomb;
    }
}
