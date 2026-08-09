using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    /// <summary>References to the instantiated, prefab-authored visual hierarchy.</summary>
    internal sealed class ZoneProgressBarElements
    {
        public RectTransform ItemsRoot { get; }
        public RectTransform CurrentMarker { get; }
        public Image CurrentMarkerImage { get; }
        public ZoneProgressBarCellView[] Cells { get; }

        public ZoneProgressBarElements(
            RectTransform itemsRoot,
            RectTransform currentMarker,
            Image currentMarkerImage,
            ZoneProgressBarCellView[] cells)
        {
            ItemsRoot = itemsRoot;
            CurrentMarker = currentMarker;
            CurrentMarkerImage = currentMarkerImage;
            Cells = cells;
        }
    }
}
