using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VertigoCase.Config;
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

        private RectTransform IconRect => iconImage != null ? iconImage.rectTransform : null;
        private RectTransform AmountRect => amountText != null ? amountText.rectTransform : null;

        // Called by WheelView for each slice. The view is dumb: it just shows what the data says.
        public void Bind(WheelSlice slice)
        {
            if (slice == null || slice.reward == null) return;

            bool isBomb = slice.IsBomb;
            ConfigureVisuals(isBomb);
            iconImage.sprite = slice.reward.icon;
            float visibleIconHeight = NormalizeIconSize(slice.reward.icon);

            // Final amount = reward base scaled by the slice multiplier.
            int amount = slice.reward.baseAmount * slice.multiplier;

            // The bomb shows only its icon. Rewards use the reference style: x10, x300, x1.5K.
            amountText.text = isBomb
                ? string.Empty
                : WheelRewardLabelFormatter.Format(amount);

            AlignContent(isBomb, visibleIconHeight);
        }

        private void ConfigureVisuals(bool isBomb)
        {
            if (iconImage != null)
            {
                iconImage.preserveAspect = true;
                iconImage.useSpriteMesh = true;
                iconImage.raycastTarget = false;

                RectTransform iconRect = IconRect;
                iconRect.sizeDelta = new Vector2(
                    GameConstants.Wheel.SliceIconBoxWidth,
                    GameConstants.Wheel.SliceIconBoxHeight);
                iconRect.localScale = Vector3.one;
            }

            if (amountText != null)
            {
                amountText.enableAutoSizing = true;
                amountText.enableWordWrapping = false;
                amountText.fontStyle |= FontStyles.Bold;
                amountText.fontSize = GameConstants.Wheel.SliceLabelFontSize;
                amountText.fontSizeMin = GameConstants.Wheel.SliceLabelMinFontSize;
                amountText.fontSizeMax = GameConstants.Wheel.SliceLabelMaxFontSize;
                amountText.alignment = TextAlignmentOptions.Center;
                amountText.raycastTarget = false;
                amountText.gameObject.SetActive(!isBomb);

                RectTransform amountRect = AmountRect;
                amountRect.sizeDelta = new Vector2(
                    GameConstants.Wheel.SliceLabelWidth,
                    GameConstants.Wheel.SliceLabelHeight);
            }
        }

        /// <summary>
        /// Sprite textures have different transparent margins. Tight-mesh sprite bounds describe
        /// the visible content, so scale against those bounds instead of treating every source
        /// texture as if it were equally full. This keeps cash, chests, weapons and the bomb
        /// visually consistent without reward-type switches or per-icon magic values.
        /// </summary>
        private float NormalizeIconSize(Sprite sprite)
        {
            RectTransform iconRect = IconRect;
            if (sprite == null || iconRect == null)
                return GameConstants.Wheel.SliceVisibleIconHeight;

            Vector2 sourceSize = sprite.rect.size;
            Vector2 visibleSize = sprite.bounds.size * sprite.pixelsPerUnit;
            if (sourceSize.x <= 0f || sourceSize.y <= 0f ||
                visibleSize.x <= 0f || visibleSize.y <= 0f)
            {
                iconRect.localScale = Vector3.one;
                return GameConstants.Wheel.SliceVisibleIconHeight;
            }

            float sourceAspect = sourceSize.x / sourceSize.y;
            float boxAspect =
                GameConstants.Wheel.SliceIconBoxWidth /
                GameConstants.Wheel.SliceIconBoxHeight;

            float renderedWidth;
            float renderedHeight;
            if (sourceAspect > boxAspect)
            {
                renderedWidth = GameConstants.Wheel.SliceIconBoxWidth;
                renderedHeight = renderedWidth / sourceAspect;
            }
            else
            {
                renderedHeight = GameConstants.Wheel.SliceIconBoxHeight;
                renderedWidth = renderedHeight * sourceAspect;
            }

            float visibleWidth = renderedWidth * visibleSize.x / sourceSize.x;
            float visibleHeight = renderedHeight * visibleSize.y / sourceSize.y;
            float widthScale = GameConstants.Wheel.SliceVisibleIconWidth / visibleWidth;
            float heightScale = GameConstants.Wheel.SliceVisibleIconHeight / visibleHeight;
            float scale = Mathf.Clamp(
                Mathf.Min(widthScale, heightScale),
                GameConstants.Wheel.SliceMinIconScale,
                GameConstants.Wheel.SliceMaxIconScale);

            iconRect.localScale = Vector3.one * scale;
            return visibleHeight * scale;
        }

        /// <summary>
        /// Treats the icon and its value as one vertically centered unit. The label position is
        /// derived from the icon's visible height, so wide and tall source sprites keep the same
        /// optical gap instead of drifting inside their wheel circles.
        /// </summary>
        private void AlignContent(bool isBomb, float visibleIconHeight)
        {
            RectTransform iconRect = IconRect;
            RectTransform amountRect = AmountRect;
            if (iconRect == null || amountRect == null) return;

            if (isBomb)
            {
                iconRect.anchoredPosition =
                    new Vector2(0f, GameConstants.Wheel.SliceContentOffsetY);
                amountRect.anchoredPosition = Vector2.zero;
                return;
            }

            float iconY = GameConstants.Wheel.SliceContentOffsetY +
                          (GameConstants.Wheel.SliceLabelVisualHeight +
                           GameConstants.Wheel.SliceContentGap) * 0.5f;
            float labelY = GameConstants.Wheel.SliceContentOffsetY -
                           (visibleIconHeight +
                            GameConstants.Wheel.SliceContentGap) * 0.5f;

            iconRect.anchoredPosition = new Vector2(0f, iconY);
            amountRect.anchoredPosition = new Vector2(0f, labelY);
        }
    }
}
