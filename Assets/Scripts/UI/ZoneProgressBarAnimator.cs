using System;
using DG.Tweening;
using UnityEngine;
using VertigoCase.Config;

namespace VertigoCase.UI
{
    /// <summary>Owns every DOTween transition used by the zone progress bar.</summary>
    internal sealed class ZoneProgressBarAnimator
    {
        private readonly ZoneProgressBarElements elements;
        private Tween activeTransition;

        public ZoneProgressBarAnimator(ZoneProgressBarElements elements)
        {
            this.elements = elements;
        }

        public void SlideStrip(float targetX, float duration, Action onComplete)
        {
            if (duration <= 0f)
            {
                elements.ItemsRoot.anchoredPosition = Vector2.zero;
                onComplete?.Invoke();
                return;
            }

            activeTransition = elements.ItemsRoot
                .DOAnchorPosX(targetX, duration)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() =>
                {
                    elements.ItemsRoot.anchoredPosition = Vector2.zero;
                    activeTransition = null;
                    onComplete?.Invoke();
                });
        }

        public void MoveMarker(
            float targetX,
            Color targetColor,
            float duration,
            bool animate,
            Action onComplete)
        {
            elements.CurrentMarker.DOKill();
            elements.CurrentMarkerImage.DOKill();

            if (!animate || duration <= 0f)
            {
                elements.CurrentMarker.anchoredPosition = new Vector2(
                    targetX,
                    elements.CurrentMarker.anchoredPosition.y);
                elements.CurrentMarkerImage.color = targetColor;
                onComplete?.Invoke();
                return;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Insert(
                0f,
                elements.CurrentMarker
                    .DOAnchorPosX(targetX, duration)
                    .SetEase(Ease.OutCubic));
            sequence.Insert(0f, elements.CurrentMarkerImage.DOColor(targetColor, duration));
            activeTransition = sequence.OnComplete(() =>
            {
                activeTransition = null;
                onComplete?.Invoke();
            });
        }

        public void PulseMarker()
        {
            elements.CurrentMarker.DOKill();
            elements.CurrentMarker.localScale = Vector3.one;
            elements.CurrentMarker.DOPunchScale(
                Vector3.one * GameConstants.ZoneBar.MarkerPulseScale,
                GameConstants.ZoneBar.MarkerPulseSeconds,
                GameConstants.ZoneBar.MarkerPulseVibrato,
                GameConstants.ZoneBar.MarkerPulseElasticity);
        }

        public void Stop(bool complete)
        {
            Tween transition = activeTransition;
            activeTransition = null;
            if (transition != null && transition.IsActive())
                transition.Kill(complete);

            elements.ItemsRoot.DOKill(complete);
            elements.ItemsRoot.anchoredPosition = Vector2.zero;

            elements.CurrentMarker.DOKill(complete);
            elements.CurrentMarker.localScale = Vector3.one;
            elements.CurrentMarkerImage.DOKill(complete);
        }
    }
}
