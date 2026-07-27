using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VertigoCase.Config;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// Applies a WheelData asset to the wheel's visuals. This is a pure View: it only displays
    /// sprites and lays out the slices; it knows nothing about spin math or rewards. It binds the
    /// body + indicator sprites and spawns one SliceView per slice evenly around a circle.
    /// </summary>
    public class WheelView : MonoBehaviour, IWheelPresenter
    {
        [SerializeField] private WheelData wheelData;   // which wheel data to show
        [SerializeField] private Image baseImage;       // ui_image_wheel_base
        [SerializeField] private Image indicatorImage;  // ui_image_wheel_indicator

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI titleText;         // ui_text_wheel_title_value
        [SerializeField] private TextMeshProUGUI rewardCalloutText; // ui_text_wheel_reward_value

        [Header("Slice layout")]
        [SerializeField] private SliceView slicePrefab;        // Prefabs/UI/SliceView_Root
        [SerializeField] private RectTransform sliceContainer; // parent the spawned slices live under
        [SerializeField] private float radius = GameConstants.Wheel.DefaultSliceRadius;

        // Refresh whenever the object becomes active (e.g. a panel opens).
        private void OnEnable() => Apply();

        // Switch to a different wheel at runtime (e.g. entering a safe/super zone) and redraw it.
        public void SetWheel(WheelData data)
        {
            wheelData = data;
            Apply();
        }

        // Push the data's sprites onto the Image components. Null-guards keep the scene from
        // throwing if a reference is missing during setup.
        public void Apply()
        {
            if (wheelData == null) return;

            if (baseImage != null && wheelData.baseSprite != null)
            {
                baseImage.sprite = wheelData.baseSprite;
                ConfigureContentImage(baseImage);
            }

            if (indicatorImage != null && wheelData.indicatorSprite != null)
            {
                indicatorImage.sprite = wheelData.indicatorSprite;
                ConfigureContentImage(indicatorImage);
            }

            ApplyLabels();
            BuildSlices();
        }

        private void ApplyLabels()
        {
            ApplyLabel(
                titleText,
                wheelData.titleLabel,
                wheelData.labelColor,
                GameConstants.Wheel.TitleLabelFontSize);
            ApplyLabel(
                rewardCalloutText,
                wheelData.rewardCalloutLabel,
                wheelData.labelColor,
                GameConstants.Wheel.RewardCalloutFontSize);
        }

        // Spawns one SliceView per slice and places them evenly around a circle.
        private void BuildSlices()
        {
            if (wheelData == null || slicePrefab == null || sliceContainer == null) return;

            // Clear any slices from a previous build, so repeated Apply() calls don't stack duplicates.
            for (int i = sliceContainer.childCount - 1; i >= 0; i--)
                Destroy(sliceContainer.GetChild(i).gameObject);

            int count = wheelData.slices.Count;
            if (count == 0) return;

            float step = GameConstants.Wheel.FullRotationDegrees / count;

            for (int i = 0; i < count; i++)
            {
                SliceView view = Instantiate(slicePrefab, sliceContainer);

                float angle = i * step;
                float rad = angle * Mathf.Deg2Rad;

                // 0 degrees points up (toward the indicator), then goes clockwise: (sin, cos).
                RectTransform rt = (RectTransform)view.transform;
                rt.anchoredPosition = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * radius;
                rt.localRotation = Quaternion.Euler(0f, 0f, -angle); // keep the icon aligned with its slice

                view.Bind(wheelData.slices[i]);
            }
        }

        private static void ConfigureContentImage(Image image)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static void ApplyLabel(
            TextMeshProUGUI label,
            string value,
            Color color,
            float fontSize)
        {
            if (label == null) return;

            bool hasValue = !string.IsNullOrWhiteSpace(value);
            label.gameObject.SetActive(hasValue);
            if (!hasValue) return;

            label.text = value;
            label.color = color;
            label.fontStyle |= FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.enableAutoSizing = true;
            label.fontSize = fontSize;
            label.fontSizeMin = GameConstants.Wheel.LabelMinFontSize;
            label.fontSizeMax = GameConstants.Wheel.LabelMaxFontSize;
            label.raycastTarget = false;
            label.maskable = false;
        }
    }
}
