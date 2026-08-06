using System;
using DG.Tweening;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Data;
using VertigoCase.Utils;

namespace VertigoCase.Wheel
{
    /// <summary>
    /// Spins the wheel with DOTween and stops with the winning slice under the indicator.
    /// SliceResolver decides the outcome first (pure logic); this MonoBehaviour only animates to it
    /// and raises events. Views listen to those events and never read the spin math (Observer + DIP).
    /// </summary>
    public class SpinController : MonoBehaviour, IWheelSpinner
    {
        [Header("References")]
        [SerializeField] private RectTransform wheelRoot;  // ONLY the rotating body. The indicator must live OUTSIDE this.
        [SerializeField] private WheelData wheelData;

        [Header("Tuning")]
        [SerializeField] private float duration = GameConstants.Wheel.DefaultSpinDurationSeconds;
        [SerializeField] private int extraSpins = GameConstants.Wheel.DefaultExtraSpins;
        [SerializeField] private float angleOffset = GameConstants.Wheel.DefaultAngleOffsetDegrees;

        [Header("Debug")]
        [SerializeField] private bool logResult = true;    // print the landed slice to the Console while testing

        public bool IsSpinning { get; private set; }       // state guard: blocks a second spin mid-animation

        private Tween spinTween;
        private WheelSlice pendingResult;

        // Observer: logic finishes, listeners (UI / VFX / economy) react. The view does not know the spin math.
        public event Action OnSpinStarted;
        public event Action<WheelSlice> OnSpinCompleted;

        // Swap the wheel this controller resolves and rotates. Must stay in sync with WheelView's data.
        public void SetWheel(WheelData data) => wheelData = data;

        [ContextMenu("Test Spin")] // right-click the component header in Play mode to fire a spin without UI
        public void Spin()
        {
            if (IsSpinning || wheelData == null || wheelRoot == null) return; // guard: don't start while spinning
            if (wheelData.slices.Count == 0) return;

            int index = SliceResolver.Resolve(wheelData);  // decide the winner FIRST, then animate to it
            pendingResult = wheelData.slices[index];
            float step = GameConstants.Wheel.FullRotationDegrees / wheelData.slices.Count;

            // WheelView places slice 0 at the top and increases index CLOCKWISE, while Unity's +Z is
            // COUNTER-clockwise. So a positive Z of index*step brings slice 'index' back under the indicator.
            // Extra spins add complete rotations and keep the target larger than the current angle,
            // so the wheel always rotates forward.
            float targetZ =
                extraSpins * GameConstants.Wheel.FullRotationDegrees + index * step + angleOffset;

            IsSpinning = true;
            spinTween = wheelRoot
                .DOLocalRotate(
                    new Vector3(0f, 0f, targetZ),
                    duration,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(HandleSpinCompleted);

            OnSpinStarted?.Invoke();
        }

        private void HandleSpinCompleted()
        {
            WheelSlice result = pendingResult;
            pendingResult = null;
            spinTween = null;
            IsSpinning = false;

            if (result == null)
                return;

            if (logResult)
                GameLog.Info(
                    $"[SpinController] Landed on '{(result.IsBomb ? "Bomb" : result.reward != null ? result.reward.name : "null")}' (bomb={result.IsBomb})",
                    this);

            OnSpinCompleted?.Invoke(result);
        }

        private void OnDisable() => CancelActiveSpin();

        private void OnDestroy() => CancelActiveSpin();

        private void CancelActiveSpin()
        {
            Tween tween = spinTween;
            spinTween = null;
            pendingResult = null;
            IsSpinning = false;

            if (tween != null && tween.IsActive())
                tween.Kill(false);
        }
    }
}
