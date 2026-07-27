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

        [Header("Labels")]
        [Tooltip("Headline displayed above this wheel, e.g. 'GOLDEN SPIN'")]
        public string titleLabel;

        [Tooltip("Reward callout displayed below this wheel, e.g. 'Up To x10 Rewards'")]
        public string rewardCalloutLabel;

        [Tooltip("Shared color used by the wheel headline and reward callout")]
        public Color labelColor = new Color(1f, 0.82f, 0f, 1f);

        [Tooltip("The slices placed around the wheel")]
        public List<WheelSlice> slices = new List<WheelSlice>();
    }
}
