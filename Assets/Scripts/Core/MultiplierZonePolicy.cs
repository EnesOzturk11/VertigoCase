using UnityEngine;

namespace VertigoCase.Core
{
    /// <summary>
    /// Concrete, data-driven zone policy. The legacy class name is retained so existing Unity
    /// assets keep their script reference; reward multipliers live only on WheelSlice data.
    /// </summary>
    [CreateAssetMenu(fileName = "ZonePolicy_", menuName = "Vertigo/Zone Policy/Multiplier")]
    public sealed class MultiplierZonePolicy : ZonePolicy
    {
    }
}
