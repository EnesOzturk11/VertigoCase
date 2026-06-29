using System.Collections.Generic;
using UnityEngine;

namespace VertigoCase.Data
{
    /// <summary>
    /// The full content of one wheel (its sprites and its list of slices). This is where the case
    /// rule "content of slices should be changeable from the editor" is satisfied: edit the asset,
    /// no code change needed. Bronze = normal (with bomb), Silver = safe, Golden = super.
    /// </summary>
    [CreateAssetMenu(fileName = "Wheel_", menuName = "Vertigo/Wheel Data")]
    public class WheelData : ScriptableObject
    {
        [Tooltip("Display name, e.g. 'Bronze' / 'Silver' / 'Golden'")]
        public string wheelName;

        [Tooltip("Wheel body sprite")]
        public Sprite baseSprite;

        [Tooltip("Pointer/indicator sprite")]
        public Sprite indicatorSprite;

        [Tooltip("Whether this wheel contains a bomb (false for Safe/Super wheels)")]
        public bool hasBomb = true;

        [Tooltip("The slices placed around the wheel (8 expected)")]
        public List<WheelSlice> slices = new List<WheelSlice>();
    }
}
