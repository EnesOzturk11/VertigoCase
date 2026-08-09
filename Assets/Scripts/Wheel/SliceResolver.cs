using System;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    /// <summary>
    /// Picks the winning slice of a wheel using weighted random. The outcome is decided here first;
    /// SpinController then animates the wheel to the selected index.
    /// </summary>
    public static class SliceResolver
    {
        // Returns the index of the chosen slice. A higher slice weight means a higher chance.
        public static int Resolve(WheelData wheel)
        {
            if (wheel == null)
                throw new ArgumentNullException(nameof(wheel));
            if (wheel.Slices == null || wheel.Slices.Count == 0)
                throw new InvalidOperationException("A wheel must contain at least one slice.");

            int total = 0;
            for (int i = 0; i < wheel.Slices.Count; i++)
            {
                WheelSlice slice = wheel.Slices[i] ??
                                   throw new InvalidOperationException(
                                       $"Wheel slice {i} is missing.");
                if (slice.Weight < GameConstants.Wheel.MinimumSliceWeight)
                    throw new InvalidOperationException(
                        $"Wheel slice {i} has an invalid selection weight.");

                total = checked(total + slice.Weight);
            }

            // Pick a point in [0, total) and walk the slices until that point falls inside one's window.
            int roll = UnityEngine.Random.Range(0, total);
            int acc = 0;
            for (int i = 0; i < wheel.Slices.Count; i++)
            {
                acc += wheel.Slices[i].Weight;
                if (roll < acc) return i;
            }

            return wheel.Slices.Count - 1; // safety net; unreachable when slices is non-empty
        }
    }
}
