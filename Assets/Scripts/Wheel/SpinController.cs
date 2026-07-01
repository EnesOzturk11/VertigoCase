using System;
using DG.Tweening;
using UnityEngine;
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    /// <summary>
    /// Spins the wheel with DOTween and stops with the winning slice under the indicator.
    /// SliceResolver decides the outcome first (pure logic); this MonoBehaviour only animates to it
    /// and raises events. Views listen to those events and never read the spin math (Observer + DIP).
    /// </summary>
    public class SpinController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform wheelRoot;  // ONLY the rotating body. The indicator must live OUTSIDE this.
        [SerializeField] private WheelData wheelData;

        [Header("Tuning")]
        [SerializeField] private float duration = 4f;      // total spin time in seconds
        [SerializeField] private int extraSpins = 5;       // full turns added for the "wheel of fortune" feel
        [SerializeField] private float angleOffset = 0f;   // fine-tune only if the indicator doesn't sit on slice centers

        [Header("Debug")]
        [SerializeField] private bool logResult = true;    // print the landed slice to the Console while testing

        public bool IsSpinning { get; private set; }       // state guard: blocks a second spin mid-animation

        // Observer: logic finishes, listeners (UI / VFX / economy) react. The view does not know the spin math.
        public event Action OnSpinStarted;
        public event Action<WheelSlice> OnSpinCompleted;

        private void Awake()
        {
            // A tiny built-in listener so we can verify this checkpoint before any UI exists.
            if (logResult)
                OnSpinCompleted += slice =>
                    Debug.Log($"[SpinController] Landed on '{(slice.reward != null ? slice.reward.name : "null")}' (bomb={slice.IsBomb})");
        }

        // Swap the wheel this controller resolves and rotates. Must stay in sync with WheelView's data.
        public void SetWheel(WheelData data) => wheelData = data;

        [ContextMenu("Test Spin")] // right-click the component header in Play mode to fire a spin without UI
        public void Spin()
        {
            if (IsSpinning || wheelData == null || wheelRoot == null) return; // guard: don't start while spinning
            if (wheelData.slices.Count == 0) return;

            IsSpinning = true;
            OnSpinStarted?.Invoke();

            int index = SliceResolver.Resolve(wheelData);  // decide the winner FIRST, then animate to it
            float step = 360f / wheelData.slices.Count;     // angle between two slices (8 slices -> 45 degrees)

            // WheelView places slice 0 at the top and increases index CLOCKWISE, while Unity's +Z is
            // COUNTER-clockwise. So a positive Z of index*step brings slice 'index' back under the indicator.
            // extraSpins*360 only adds full turns on top, and keeps the target larger than the current angle
            // so the wheel always rotates forward.
            float targetZ = extraSpins * 360f + index * step + angleOffset;

            wheelRoot.DOLocalRotate(new Vector3(0f, 0f, targetZ), duration, RotateMode.FastBeyond360)
                     .SetEase(Ease.OutCubic)                // decelerate smoothly toward the end
                     .OnComplete(() =>
                     {
                         IsSpinning = false;
                         OnSpinCompleted?.Invoke(wheelData.slices[index]);
                     });
        }
    }
}
