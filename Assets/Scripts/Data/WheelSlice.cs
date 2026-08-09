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
    public sealed class WheelSlice
    {
        [Tooltip("Collectible reward. Leave empty for a hazard slice.")]
        [SerializeField] private RewardData reward;

        [Tooltip("Whether this slice ends the run instead of granting a reward.")]
        [SerializeField] private bool isBomb;

        [Tooltip("Visual used by a hazard slice.")]
        [SerializeField] private Sprite hazardIcon;

        [Tooltip("Multiplier used to calculate both the displayed and awarded amount.")]
        [SerializeField, Min(0)]
        private int multiplier = GameConstants.Wheel.DefaultSliceMultiplier;

        [Tooltip("Selection weight. Higher = comes up more often. Rare rewards / the bomb get a lower weight.")]
        [SerializeField, Min(GameConstants.Wheel.MinimumSliceWeight)]
        private int weight = GameConstants.Wheel.DefaultSliceWeight;

        public RewardData Reward => reward;
        public bool IsBomb => isBomb;
        public Sprite HazardIcon => hazardIcon;
        public int Multiplier => multiplier;
        public int Weight => weight;
        public Sprite Icon => IsBomb ? hazardIcon : reward != null ? reward.Icon : null;
    }
}
