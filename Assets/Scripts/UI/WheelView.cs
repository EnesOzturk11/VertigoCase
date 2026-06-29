using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// Applies a WheelData asset to the wheel's visuals. This is a pure View: it only displays
    /// sprites, it does not know about spin math or rewards. The circular slice layout and the
    /// spin animation come on Day 3 (math + DOTween). For now it just binds body + indicator.
    /// </summary>
    public class WheelView : MonoBehaviour
    {
        [SerializeField] private WheelData wheelData;   // which wheel data to show
        [SerializeField] private Image baseImage;       // ui_image_wheel_base
        [SerializeField] private Image indicatorImage;  // ui_image_wheel_indicator

        // Refresh whenever the object becomes active (e.g. a panel opens).
        private void OnEnable() => Apply();

        // Push the data's sprites onto the Image components. Null-guards keep the scene from
        // throwing if a reference is missing during setup.
        public void Apply()
        {
            if (wheelData == null) return;

            if (baseImage != null && wheelData.baseSprite != null)
                baseImage.sprite = wheelData.baseSprite;

            if (indicatorImage != null && wheelData.indicatorSprite != null)
                indicatorImage.sprite = wheelData.indicatorSprite;
        }
    }
}
