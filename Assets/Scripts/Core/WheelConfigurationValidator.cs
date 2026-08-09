using System;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    /// <summary>
    /// Validates wheel authoring independently from zone routing. Runtime consumers can rely on
    /// non-null slices, valid reward values and a selection total that fits the resolver math.
    /// </summary>
    internal static class WheelConfigurationValidator
    {
        public static void Validate(WheelData wheel, int requiredBombCount, string policyName)
        {
            if (wheel == null)
                throw new ArgumentNullException(nameof(wheel));
            if (requiredBombCount < 0)
                throw new InvalidOperationException(
                    $"{policyName} cannot require a negative bomb count.");
            if (string.IsNullOrWhiteSpace(wheel.WheelName))
                throw new InvalidOperationException($"{wheel.name} requires a display name.");
            if (wheel.BaseSprite == null || wheel.IndicatorSprite == null)
                throw new InvalidOperationException(
                    $"{wheel.name} requires both wheel and indicator sprites.");
            if (wheel.Slices == null || wheel.Slices.Count == 0)
                throw new InvalidOperationException($"{wheel.name} must contain at least one slice.");

            int bombCount = 0;
            int totalWeight = 0;
            for (int i = 0; i < wheel.Slices.Count; i++)
            {
                WheelSlice slice = wheel.Slices[i];
                if (slice == null)
                    throw new InvalidOperationException($"{wheel.name} has a missing slice at index {i}.");
                if (slice.Weight < GameConstants.Wheel.MinimumSliceWeight)
                    throw new InvalidOperationException(
                        $"{wheel.name} slice {i} has an invalid selection weight.");

                try
                {
                    totalWeight = checked(totalWeight + slice.Weight);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidOperationException(
                        $"{wheel.name} slice weights exceed the supported selection range.",
                        exception);
                }

                if (slice.IsBomb)
                {
                    bombCount++;
                    ValidateBombSlice(wheel, slice, i);
                }
                else
                {
                    ValidateRewardSlice(wheel, slice, i);
                }
            }

            if (bombCount != requiredBombCount)
                throw new InvalidOperationException(
                    $"{policyName} requires exactly {requiredBombCount} bomb slice(s), " +
                    $"but {wheel.name} contains {bombCount}.");
        }

        private static void ValidateBombSlice(WheelData wheel, WheelSlice slice, int index)
        {
            if (slice.Reward != null)
                throw new InvalidOperationException(
                    $"{wheel.name} bomb slice {index} cannot contain a collectible reward.");
            if (slice.HazardIcon == null)
                throw new InvalidOperationException(
                    $"{wheel.name} bomb slice {index} does not have a hazard icon.");
            if (slice.Multiplier != 0)
                throw new InvalidOperationException(
                    $"{wheel.name} bomb slice {index} must use a zero multiplier.");
        }

        private static void ValidateRewardSlice(WheelData wheel, WheelSlice slice, int index)
        {
            if (slice.Reward == null)
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} does not reference a reward.");
            if (slice.HazardIcon != null)
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} cannot contain a hazard icon.");
            if (!Enum.IsDefined(typeof(RewardType), slice.Reward.Type))
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} has an unknown reward type.");
            if (slice.Reward.Icon == null)
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} does not have an icon.");
            if (slice.Reward.BaseAmount <= 0)
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} has an invalid base amount.");
            if (slice.Multiplier < GameConstants.Wheel.DefaultSliceMultiplier)
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} has an invalid multiplier.");

            try
            {
                _ = checked(slice.Reward.BaseAmount * slice.Multiplier);
            }
            catch (OverflowException exception)
            {
                throw new InvalidOperationException(
                    $"{wheel.name} reward slice {index} exceeds the supported reward range.",
                    exception);
            }
        }
    }
}
