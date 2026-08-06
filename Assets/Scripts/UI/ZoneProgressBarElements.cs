using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoCase.UI
{
    /// <summary>References to the generated visual hierarchy.</summary>
    internal sealed class ZoneProgressBarElements
    {
        public RectTransform ItemsRoot { get; }
        public RectTransform CurrentMarker { get; }
        public Image CurrentMarkerImage { get; }
        public Image[] CellImages { get; }
        public TextMeshProUGUI[] Labels { get; }

        public ZoneProgressBarElements(
            RectTransform itemsRoot,
            RectTransform currentMarker,
            Image currentMarkerImage,
            Image[] cellImages,
            TextMeshProUGUI[] labels)
        {
            ItemsRoot = itemsRoot;
            CurrentMarker = currentMarker;
            CurrentMarkerImage = currentMarkerImage;
            CellImages = cellImages;
            Labels = labels;
        }
    }
}
