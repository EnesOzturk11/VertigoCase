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
    public sealed class WheelData : ScriptableObject
    {
        [Tooltip("Display name, e.g. 'Bronze' / 'Silver' / 'Golden'")]
        [SerializeField] private string wheelName;

        [Tooltip("Wheel body sprite")]
        [SerializeField] private Sprite baseSprite;

        [Tooltip("Pointer/indicator sprite")]
        [SerializeField] private Sprite indicatorSprite;

        [Header("Labels")]
        [Tooltip("Headline displayed above this wheel, e.g. 'GOLDEN SPIN'")]
        [SerializeField] private string titleLabel;

        [Tooltip("Color used by the wheel headline")]
        [SerializeField] private Color labelColor = new Color(1f, 0.82f, 0f, 1f);

        [Tooltip("The slices placed around the wheel")]
        [SerializeField] private List<WheelSlice> slices = new List<WheelSlice>();

        public string WheelName => wheelName;
        public Sprite BaseSprite => baseSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public string TitleLabel => titleLabel;
        public Color LabelColor => labelColor;
        public IReadOnlyList<WheelSlice> Slices => slices;
    }
}
