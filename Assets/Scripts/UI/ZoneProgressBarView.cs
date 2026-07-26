using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Config;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Displays a moving window of zones. The current-zone marker stays centered while the number
    /// strip slides underneath it; slots before zone one remain empty.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneProgressBarView : MonoBehaviour
    {
        private static readonly Color BarTint = new Color(0.01f, 0.01f, 0.014f, 0.98f);
        private static readonly Color CellTint = Color.clear;
        private static readonly Color FrameBlack = new Color(0.005f, 0.005f, 0.007f, 1f);
        private static readonly Color FutureText = new Color(0.96f, 0.96f, 0.96f, 1f);
        private static readonly Color PastText = new Color(0.42f, 0.42f, 0.45f, 0.72f);
        private static readonly Color SafeText = new Color(0.48f, 1f, 0.04f, 1f);
        private static readonly Color SuperText = new Color(1f, 0.67f, 0.05f, 1f);
        private static readonly Color CurrentSafeText = new Color(0.76f, 1f, 0.48f, 1f);
        private static readonly Color CurrentSuperText = new Color(1f, 0.88f, 0.48f, 1f);
        private static readonly Color NormalMarker = new Color(0.93f, 0.93f, 0.96f, 1f);
        private static readonly Color SafeMarker = new Color(0.16f, 0.48f, 0.01f, 1f);
        private static readonly Color SuperMarker = new Color(0.55f, 0.29f, 0.01f, 1f);

        [Header("Source")]
        [SerializeField] private MonoBehaviour game;

        [Header("Marker Asset")]
        [SerializeField] private Sprite currentMarkerSprite;
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite indicatorSprite;

        [Header("Layout")]
        [SerializeField, Min(5)] private int visibleZoneCount = GameConstants.ZoneBar.VisibleZoneCount;
        [SerializeField, Min(1f)] private float cellWidth = GameConstants.ZoneBar.CellWidth;
        [SerializeField, Min(0f)] private float cellSpacing = GameConstants.ZoneBar.CellSpacing;
        [SerializeField, Min(1f)] private float barHeight = GameConstants.ZoneBar.BarHeight;
        [SerializeField, Min(1f)] private float currentMarkerHeight =
            GameConstants.ZoneBar.CurrentMarkerHeight;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float transitionSeconds =
            GameConstants.ZoneBar.TransitionSeconds;

        private IZonePort gamePort;
        private RectTransform itemsRoot;
        private RectTransform currentMarker;
        private Image currentMarkerImage;
        private Image[] cellImages;
        private TextMeshProUGUI[] labels;
        private Tween transition;
        private int currentZone;
        private int firstVisibleZone = GameConstants.Zones.FirstZone;
        private bool hasZone;

        private float CellStride => cellWidth + cellSpacing;
        private int CenterIndex => visibleZoneCount / 2;

        private void Awake()
        {
            gamePort = game as IZonePort ??
                       throw new InvalidOperationException(
                           "ZoneProgressBarView requires a component implementing IZonePort.");

            if (visibleZoneCount % 2 == 0)
                visibleZoneCount++;
            if (currentMarkerSprite == null)
                throw new InvalidOperationException("Zone bar rounded panel sprite is missing.");
            if (frameSprite == null)
                throw new InvalidOperationException("Zone bar frame sprite is missing.");
            if (indicatorSprite == null)
                throw new InvalidOperationException("Zone bar indicator sprite is missing.");

            BuildVisuals();
        }

        private void OnEnable() => gamePort.OnZoneChanged += HandleZoneChanged;

        private void OnDisable()
        {
            if (gamePort != null)
                gamePort.OnZoneChanged -= HandleZoneChanged;

            KillAnimations(false);
        }

        private void HandleZoneChanged(int zone, ZoneType type)
        {
            if (zone < GameConstants.Zones.FirstZone)
                throw new ArgumentOutOfRangeException(nameof(zone), zone, "Zone must be positive.");

            KillAnimations(true);

            if (!hasZone)
            {
                hasZone = true;
                currentZone = zone;
                firstVisibleZone = CalculateFirstVisibleZone(zone);
                RefreshStrip();
                SetMarkerPosition(false);
                return;
            }

            int nextFirstVisibleZone = CalculateFirstVisibleZone(zone);
            bool isSequentialAdvance = zone == currentZone + 1;
            bool shouldSlide = isSequentialAdvance &&
                               nextFirstVisibleZone == firstVisibleZone + 1;

            if (shouldSlide)
            {
                AnimateSlidingWindow(zone, nextFirstVisibleZone);
                return;
            }

            currentZone = zone;
            firstVisibleZone = nextFirstVisibleZone;
            RefreshStrip();
            SetMarkerPosition(isSequentialAdvance);
        }

        private void AnimateSlidingWindow(int zone, int nextFirstVisibleZone)
        {
            transition = itemsRoot
                .DOAnchorPosX(-CellStride, transitionSeconds)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() =>
                {
                    currentZone = zone;
                    firstVisibleZone = nextFirstVisibleZone;
                    itemsRoot.anchoredPosition = Vector2.zero;
                    RefreshStrip();
                    SetMarkerPosition(false);
                    PulseMarker();
                    transition = null;
                });
        }

        private void RefreshStrip()
        {
            for (int i = 0; i < visibleZoneCount; i++)
            {
                int zone = firstVisibleZone + i;

                if (zone < GameConstants.Zones.FirstZone)
                {
                    labels[i].text = string.Empty;
                    labels[i].color = Color.clear;
                    cellImages[i].color = Color.clear;
                    continue;
                }

                ZoneType type = gamePort.TypeOf(zone);

                labels[i].text = zone.ToString();
                labels[i].color = ResolveTextColor(zone, type);
                cellImages[i].color = CellTint;
            }

            ZoneType currentType = gamePort.TypeOf(currentZone);
            currentMarkerImage.color = MarkerColor(currentType);

            int currentIndex = currentZone - firstVisibleZone;
            if (currentIndex >= 0 && currentIndex < labels.Length)
                labels[currentIndex].color = CurrentTextColor(currentType);
        }

        private void SetMarkerPosition(bool animate)
        {
            float targetX = CellPosition(currentZone - firstVisibleZone);
            currentMarker.DOKill();
            currentMarkerImage.DOKill();

            ZoneType currentType = gamePort.TypeOf(currentZone);
            currentMarkerImage.DOColor(MarkerColor(currentType), transitionSeconds);

            if (!animate || transitionSeconds <= 0f)
            {
                currentMarker.anchoredPosition =
                    new Vector2(targetX, currentMarker.anchoredPosition.y);
                return;
            }

            transition = currentMarker
                .DOAnchorPosX(targetX, transitionSeconds)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    PulseMarker();
                    transition = null;
                });
        }

        private void PulseMarker()
        {
            currentMarker.DOKill();
            currentMarker.localScale = Vector3.one;
            currentMarker.DOPunchScale(
                Vector3.one * 0.1f,
                GameConstants.Inventory.RowAnimationSeconds,
                5,
                0.45f);
        }

        private int CalculateFirstVisibleZone(int zone)
        {
            return zone - CenterIndex;
        }

        private float CellPosition(int index)
        {
            return (index - CenterIndex) * CellStride;
        }

        private Color ResolveTextColor(int zone, ZoneType type)
        {
            if (zone < currentZone)
            {
                if (type == ZoneType.Super)
                    return new Color(SuperText.r, SuperText.g, SuperText.b, 0.38f);
                if (type == ZoneType.Safe)
                    return new Color(SafeText.r, SafeText.g, SafeText.b, 0.38f);
                return PastText;
            }

            if (type == ZoneType.Super) return SuperText;
            if (type == ZoneType.Safe) return SafeText;
            return FutureText;
        }

        private static Color MarkerColor(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Safe:
                    return SafeMarker;
                case ZoneType.Super:
                    return SuperMarker;
                default:
                    return NormalMarker;
            }
        }

        private static Color CurrentTextColor(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Safe:
                    return CurrentSafeText;
                case ZoneType.Super:
                    return CurrentSuperText;
                default:
                    return Color.black;
            }
        }

        private void KillAnimations(bool complete)
        {
            if (transition != null && transition.IsActive())
                transition.Kill(complete);

            transition = null;

            if (itemsRoot != null)
            {
                itemsRoot.DOKill(complete);
                itemsRoot.anchoredPosition = Vector2.zero;
            }

            if (currentMarker != null)
            {
                currentMarker.DOKill(complete);
                currentMarker.localScale = Vector3.one;
            }

            if (currentMarkerImage != null)
                currentMarkerImage.DOKill(complete);
        }

        private void BuildVisuals()
        {
            RectTransform root = (RectTransform)transform;
            float width = visibleZoneCount * cellWidth +
                          (visibleZoneCount - 1) * cellSpacing +
                          cellWidth * 0.5f;
            root.sizeDelta = new Vector2(width, currentMarkerHeight);

            Image background = CreateImage(
                "ZoneBar_Background",
                transform,
                currentMarkerSprite,
                BarTint);
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.sizeDelta = new Vector2(width, barHeight);
            background.type = Image.Type.Sliced;
            background.fillCenter = true;
            CreateSpriteFrame(backgroundRect, "ZoneBar_Frame");

            currentMarkerImage = CreateImage(
                "ZoneBar_CurrentMarker",
                transform,
                currentMarkerSprite,
                NormalMarker);
            currentMarker = currentMarkerImage.rectTransform;
            currentMarker.anchorMin = currentMarker.anchorMax = new Vector2(0.5f, 0.5f);
            currentMarker.pivot = new Vector2(0.5f, 0.5f);
            currentMarker.sizeDelta = new Vector2(
                cellWidth + GameConstants.ZoneBar.CurrentMarkerWidthPadding,
                currentMarkerHeight);
            currentMarkerImage.type = Image.Type.Sliced;
            currentMarkerImage.fillCenter = true;
            CreateSpriteFrame(currentMarker, "CurrentMarker_Frame");

            Image indicator = CreateImage(
                "CurrentMarker_Indicator",
                currentMarker,
                indicatorSprite,
                Color.white);
            RectTransform indicatorRect = indicator.rectTransform;
            indicatorRect.anchorMin = indicatorRect.anchorMax = new Vector2(0.5f, 1f);
            indicatorRect.pivot = new Vector2(0.5f, 1f);
            indicatorRect.anchoredPosition =
                new Vector2(0f, GameConstants.ZoneBar.IndicatorTopOffset);
            indicatorRect.sizeDelta = new Vector2(
                GameConstants.ZoneBar.IndicatorWidth,
                GameConstants.ZoneBar.IndicatorHeight);
            // The wheel pointer is intentionally widened for this smaller use: its native,
            // tall aspect ratio looks needle-thin when placed on the zone marker.
            indicator.preserveAspect = false;

            GameObject itemsObject = new GameObject(
                "ZoneBar_Items",
                typeof(RectTransform));
            itemsObject.transform.SetParent(transform, false);
            itemsRoot = (RectTransform)itemsObject.transform;
            itemsRoot.anchorMin = itemsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            itemsRoot.pivot = new Vector2(0.5f, 0.5f);
            itemsRoot.sizeDelta = new Vector2(width, barHeight);

            cellImages = new Image[visibleZoneCount];
            labels = new TextMeshProUGUI[visibleZoneCount];

            for (int i = 0; i < visibleZoneCount; i++)
            {
                Image cell = CreateImage("Zone_" + i, itemsRoot, null, CellTint);
                RectTransform cellRect = cell.rectTransform;
                cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);
                cellRect.sizeDelta = new Vector2(cellWidth, barHeight);
                cellRect.anchoredPosition = new Vector2(CellPosition(i), 0f);
                cell.type = Image.Type.Simple;

                TextMeshProUGUI label = CreateLabel(cellRect);
                cellImages[i] = cell;
                labels[i] = label;
            }

        }
        private void CreateSpriteFrame(RectTransform parent, string frameName)
        {
            Image shadow = CreateImage(frameName + "_Black", parent, frameSprite, FrameBlack);
            Stretch(shadow.rectTransform);
            float offset = GameConstants.ZoneBar.FrameShadowOffset;
            shadow.rectTransform.offsetMin = new Vector2(-offset, -offset);
            shadow.rectTransform.offsetMax = new Vector2(offset, offset);
            shadow.type = Image.Type.Sliced;
            shadow.fillCenter = false;

            Image highlight = CreateImage(frameName + "_White", parent, frameSprite, Color.white);
            Stretch(highlight.rectTransform);
            highlight.type = Image.Type.Sliced;
            highlight.fillCenter = false;
        }

        private static Image CreateImage(
            string objectName,
            Transform parent,
            Sprite sprite,
            Color color)
        {
            GameObject imageObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent)
        {
            GameObject labelObject = new GameObject(
                "ZoneNumber",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);

            RectTransform rect = (RectTransform)labelObject.transform;
            Stretch(rect);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Normal;
            label.enableAutoSizing = true;
            label.fontSizeMin = 28f;
            label.fontSizeMax = 58f;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
