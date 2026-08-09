using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Config;

namespace VertigoCase.UI
{
    /// <summary>Owns transient reward icons and their complete DOTween lifecycle.</summary>
    internal sealed class RewardFlyAnimator
    {
        private sealed class ActiveFlight
        {
            public Sequence Sequence { get; }
            public IReadOnlyList<GameObject> Icons { get; }

            public ActiveFlight(Sequence sequence, IReadOnlyList<GameObject> icons)
            {
                Sequence = sequence;
                Icons = icons;
            }
        }

        private readonly GameObject host;
        private readonly RectTransform layer;
        private readonly RectTransform originTransform;
        private readonly int iconCount;
        private readonly Dictionary<InventoryRowView, ActiveFlight> activeFlights =
            new Dictionary<InventoryRowView, ActiveFlight>();

        public RewardFlyAnimator(
            GameObject host,
            RectTransform layer,
            RectTransform originTransform,
            int iconCount)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            this.layer = layer ?? throw new ArgumentNullException(nameof(layer));
            this.originTransform = originTransform ??
                                   throw new ArgumentNullException(nameof(originTransform));
            if (iconCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iconCount));

            this.iconCount = iconCount;
        }

        public void Play(InventoryRowView row, Sprite rewardIcon, int finalAmount)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (rewardIcon == null) throw new ArgumentNullException(nameof(rewardIcon));
            if (finalAmount <= 0) throw new ArgumentOutOfRangeException(nameof(finalAmount));

            Cancel(row);
            Canvas.ForceUpdateCanvases();

            var icons = new List<GameObject>(iconCount);
            Sequence sequence = DOTween.Sequence().SetLink(host);
            var flight = new ActiveFlight(sequence, icons);
            activeFlights.Add(row, flight);

            Vector3 origin = originTransform.position;
            Vector3 target = row.IconTarget.position;

            for (int i = 0; i < iconCount; i++)
            {
                Image icon = CreateIcon(rewardIcon, origin);
                icons.Add(icon.gameObject);
                InsertAnimation(sequence, icon, origin, target, i);
            }

            sequence.OnComplete(() => Complete(row, flight, finalAmount));
        }

        public void Cancel(InventoryRowView row)
        {
            if (row == null || !activeFlights.TryGetValue(row, out ActiveFlight flight))
                return;

            activeFlights.Remove(row);
            StopAndDestroy(flight);
        }

        public void CancelAll()
        {
            var flights = new List<ActiveFlight>(activeFlights.Values);
            activeFlights.Clear();

            for (int i = 0; i < flights.Count; i++)
                StopAndDestroy(flights[i]);
        }

        private void Complete(InventoryRowView row, ActiveFlight flight, int finalAmount)
        {
            if (!activeFlights.TryGetValue(row, out ActiveFlight current) ||
                !ReferenceEquals(current, flight))
            {
                return;
            }

            activeFlights.Remove(row);
            DestroyIcons(flight.Icons);
            if (row != null)
                row.SetAmount(finalAmount, true);
        }

        private Image CreateIcon(Sprite sprite, Vector3 origin)
        {
            var iconObject = new GameObject(
                "ui_image_reward_fly_value",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            iconObject.layer = host.layer;

            RectTransform iconTransform = iconObject.GetComponent<RectTransform>();
            iconTransform.SetParent(layer, false);
            iconTransform.SetAsLastSibling();
            iconTransform.position = origin;
            iconTransform.sizeDelta = Vector2.one * GameConstants.Inventory.RewardFlyIconSize;
            iconTransform.localScale = Vector3.zero;

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.maskable = false;
            return icon;
        }

        private void InsertAnimation(
            Sequence sequence,
            Image icon,
            Vector3 origin,
            Vector3 target,
            int index)
        {
            float delay = index * GameConstants.Inventory.RewardFlyStaggerSeconds;
            float angle = index * Mathf.PI * 2f / iconCount;
            float radius = GameConstants.Inventory.RewardFlyScatterRadius *
                           (index % 2 == 0 ? 1f : 0.72f);
            Vector3 scatterTarget = origin +
                                    new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            RectTransform iconTransform = icon.rectTransform;
            sequence.Insert(
                delay,
                iconTransform.DOScale(Vector3.one, GameConstants.Inventory.RowAnimationSeconds)
                    .SetEase(Ease.OutBack));
            sequence.Insert(
                delay,
                iconTransform.DOMove(scatterTarget, GameConstants.Inventory.RewardFlyScatterSeconds)
                    .SetEase(Ease.OutQuad));
            sequence.Insert(
                delay + GameConstants.Inventory.RewardFlyScatterSeconds,
                iconTransform.DOMove(target, GameConstants.Inventory.RewardFlyTravelSeconds)
                    .SetEase(Ease.InQuad));
            sequence.Insert(
                delay + GameConstants.Inventory.RewardFlyScatterSeconds +
                GameConstants.Inventory.RewardFlyTravelSeconds -
                GameConstants.Inventory.RewardFlyFadeSeconds,
                icon.DOFade(0f, GameConstants.Inventory.RewardFlyFadeSeconds));
        }

        private static void StopAndDestroy(ActiveFlight flight)
        {
            if (flight.Sequence != null && flight.Sequence.IsActive())
                flight.Sequence.Kill(false);

            DestroyIcons(flight.Icons);
        }

        private static void DestroyIcons(IReadOnlyList<GameObject> icons)
        {
            for (int i = icons.Count - 1; i >= 0; i--)
            {
                if (icons[i] != null)
                    UnityEngine.Object.Destroy(icons[i]);
            }
        }
    }
}
