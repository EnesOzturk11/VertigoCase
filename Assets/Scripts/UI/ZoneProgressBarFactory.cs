using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Config;

namespace VertigoCase.UI
{
    /// <summary>Builds the runtime-only hierarchy of the zone progress bar.</summary>
    internal static class ZoneProgressBarFactory
    {
        public static ZoneProgressBarElements Build(
            RectTransform root,
            ZoneProgressBarLayout layout,
            ZoneProgressBarTheme theme,
            Sprite currentMarkerSprite,
            Sprite frameSprite,
            Sprite indicatorSprite)
        {
            root.sizeDelta = new Vector2(layout.Width, layout.CurrentMarkerHeight);

            Image background = CreateImage(
                "ZoneBar_Background",
                root,
                currentMarkerSprite,
                theme.BarTint);
            ConfigureCenteredRect(background.rectTransform, layout.Width, layout.BarHeight);
            background.type = Image.Type.Sliced;
            background.fillCenter = true;
            CreateSpriteFrame(background.rectTransform, "ZoneBar_Frame", frameSprite, theme);

            Image markerImage = CreateImage(
                "ZoneBar_CurrentMarker",
                root,
                currentMarkerSprite,
                Color.white);
            RectTransform marker = markerImage.rectTransform;
            ConfigureCenteredRect(
                marker,
                layout.CellWidth + GameConstants.ZoneBar.CurrentMarkerWidthPadding,
                layout.CurrentMarkerHeight);
            markerImage.type = Image.Type.Sliced;
            markerImage.fillCenter = true;
            CreateSpriteFrame(marker, "CurrentMarker_Frame", frameSprite, theme);
            CreateIndicator(marker, indicatorSprite);

            RectTransform itemsRoot = CreateItemsRoot(root, layout);
            var cellImages = new Image[layout.VisibleZoneCount];
            var labels = new TextMeshProUGUI[layout.VisibleZoneCount];

            for (int i = 0; i < layout.VisibleZoneCount; i++)
            {
                Image cell = CreateImage("Zone_" + i, itemsRoot, null, theme.CellTint);
                RectTransform cellRect = cell.rectTransform;
                ConfigureCenteredRect(cellRect, layout.CellWidth, layout.BarHeight);
                cellRect.anchoredPosition = new Vector2(layout.CellPosition(i), 0f);

                cellImages[i] = cell;
                labels[i] = CreateLabel(cellRect);
            }

            return new ZoneProgressBarElements(
                itemsRoot,
                marker,
                markerImage,
                cellImages,
                labels);
        }

        private static RectTransform CreateItemsRoot(Transform parent, ZoneProgressBarLayout layout)
        {
            var itemsObject = new GameObject("ZoneBar_Items", typeof(RectTransform));
            itemsObject.transform.SetParent(parent, false);

            var itemsRoot = (RectTransform)itemsObject.transform;
            ConfigureCenteredRect(itemsRoot, layout.Width, layout.BarHeight);
            return itemsRoot;
        }

        private static void CreateIndicator(RectTransform marker, Sprite indicatorSprite)
        {
            Image indicator = CreateImage(
                "CurrentMarker_Indicator",
                marker,
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
            indicator.preserveAspect = true;
        }

        private static void CreateSpriteFrame(
            RectTransform parent,
            string frameName,
            Sprite frameSprite,
            ZoneProgressBarTheme theme)
        {
            Image shadow = CreateImage(
                frameName + "_Black",
                parent,
                frameSprite,
                theme.FrameShadowColor);
            Stretch(shadow.rectTransform);
            float offset = GameConstants.ZoneBar.FrameShadowOffset;
            shadow.rectTransform.offsetMin = new Vector2(-offset, -offset);
            shadow.rectTransform.offsetMax = new Vector2(offset, offset);
            shadow.type = Image.Type.Sliced;
            shadow.fillCenter = false;

            Image highlight = CreateImage(
                frameName + "_White",
                parent,
                frameSprite,
                theme.FrameHighlightColor);
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
            var imageObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            image.maskable = false;
            return image;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent)
        {
            var labelObject = new GameObject(
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
            label.maskable = false;
            return label;
        }

        private static void ConfigureCenteredRect(RectTransform rect, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
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
