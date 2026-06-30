using UnityEngine;          // Mathf + UnityEngine.Random
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    /// <summary>
    /// Picks the winning slice of a wheel using weighted random. Pure C# (static, no MonoBehaviour),
    /// so it can be unit-tested without a scene. The outcome is decided here first; the wheel is then
    /// animated to land on it (SpinController, Day 3 next step).
    /// </summary>
    public static class SliceResolver
    {
        // Returns the index of the chosen slice. A higher slice weight means a higher chance.
        public static int Resolve(WheelData wheel)
        {
            // Sum every weight (clamped to at least 1, so a zero/negative weight can't break the math).
            int total = 0;
            for (int i = 0; i < wheel.slices.Count; i++)
                total += Mathf.Max(1, wheel.slices[i].weight);

            // Pick a point in [0, total) and walk the slices until that point falls inside one's window.
            int roll = Random.Range(0, total);
            int acc = 0;
            for (int i = 0; i < wheel.slices.Count; i++)
            {
                acc += Mathf.Max(1, wheel.slices[i].weight);
                if (roll < acc) return i;
            }

            return wheel.slices.Count - 1; // safety net; unreachable when slices is non-empty
        }
    }
}
