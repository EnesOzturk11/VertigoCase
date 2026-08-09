using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Config;

namespace VertigoCase.UI
{
    /// <summary>
    /// Serialized bindings for the authored zone-bar prefab. Runtime work is limited to sizing the
    /// responsive parts and instantiating the repeated authored cell prefab.
    /// </summary>
    public sealed class ZoneProgressBarWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform itemsRoot;
        [SerializeField] private RectTransform currentMarker;
        [SerializeField] private Image currentMarkerImage;
        [SerializeField] private ZoneProgressBarCellView cellPrefab;

        internal ZoneProgressBarElements Initialize(ZoneProgressBarLayout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (background == null)
                throw new InvalidOperationException("Zone progress background is missing.");
            if (itemsRoot == null)
                throw new InvalidOperationException("Zone progress items root is missing.");
            if (currentMarker == null || currentMarkerImage == null)
                throw new InvalidOperationException("Zone progress marker references are missing.");
            if (cellPrefab == null)
                throw new InvalidOperationException("Zone progress cell prefab is missing.");

            RectTransform root = (RectTransform)transform;
            root.sizeDelta = new Vector2(layout.Width, layout.CurrentMarkerHeight);
            background.sizeDelta = new Vector2(layout.Width, layout.BarHeight);
            itemsRoot.sizeDelta = new Vector2(layout.Width, layout.BarHeight);
            currentMarker.sizeDelta = new Vector2(
                layout.CellWidth + GameConstants.ZoneBar.CurrentMarkerWidthPadding,
                layout.CurrentMarkerHeight);

            var cells = new ZoneProgressBarCellView[layout.VisibleZoneCount];
            for (int i = 0; i < cells.Length; i++)
            {
                ZoneProgressBarCellView cell = Instantiate(cellPrefab, itemsRoot, false);
                RectTransform rect = cell.RectTransform;
                rect.sizeDelta = new Vector2(layout.CellWidth, layout.BarHeight);
                rect.anchoredPosition = new Vector2(layout.CellPosition(i), 0f);
                cells[i] = cell;
            }

            return new ZoneProgressBarElements(
                itemsRoot,
                currentMarker,
                currentMarkerImage,
                cells);
        }
    }
}
