using System;
using UnityEngine;
using VertigoCase.Config;

namespace VertigoCase.Data
{
    /// <summary>
    /// A single slice on the wheel. This is NOT a ScriptableObject; it lives as a list
    /// element inside <see cref="WheelData"/>. Hazards are outcomes, not collectible reward types.
    /// </summary>
    [Serializable] // makes Unity show and edit this class in the Inspector
    public class WheelSlice
    {
        [Tooltip("Collectible reward. Leave empty for a hazard slice.")]
        public RewardData reward;

        [Tooltip("Whether this slice ends the run instead of granting a reward.")]
        public bool isBomb;

        [Tooltip("Visual used by a hazard slice.")]
        public Sprite hazardIcon;

        [Tooltip("Multiplier used to calculate both the displayed and awarded amount.")]
        public int multiplier = GameConstants.Wheel.DefaultSliceMultiplier;

        [Tooltip("Selection weight. Higher = comes up more often. Rare rewards / the bomb get a lower weight.")]
        public int weight = GameConstants.Wheel.DefaultSliceWeight;

        public bool IsBomb => isBomb;
        public Sprite Icon => IsBomb ? hazardIcon : reward != null ? reward.icon : null;
    }
}
