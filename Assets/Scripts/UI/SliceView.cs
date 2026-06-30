using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// View for a single wheel slice. It only displays one <see cref="WheelSlice"/> (icon + amount);
    /// it holds no game logic. WheelView spawns this prefab once per slice and calls <see cref="Bind"/>.
    /// </summary>
    public class SliceView : MonoBehaviour
    {
        [Tooltip("Icon image of this slice (ui_image_slice_icon)")]
        [SerializeField] private Image iconImage;

        [Tooltip("Amount label of this slice (ui_text_slice_value)")]
        [SerializeField] private TextMeshProUGUI amountText;

        // Called by WheelView for each slice. The view is dumb: it just shows what the data says.
        public void Bind(WheelSlice slice)
        {
            if (slice == null || slice.reward == null) return;

            iconImage.sprite = slice.reward.icon;

            // Final amount = reward base scaled by the slice multiplier.
            int amount = slice.reward.baseAmount * slice.multiplier;

            // The bomb shows only its icon, no number.
            amountText.text = slice.IsBomb ? string.Empty : amount.ToString();
        }
    }
}
