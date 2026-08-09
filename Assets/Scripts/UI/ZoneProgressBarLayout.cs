using System;
using VertigoCase.Config;

namespace VertigoCase.UI
{
    /// <summary>Immutable geometry calculations for the zone progress bar.</summary>
    internal sealed class ZoneProgressBarLayout
    {
        public int VisibleZoneCount { get; }
        public float CellWidth { get; }
        public float BarHeight { get; }
        public float CurrentMarkerHeight { get; }
        public float CellStride { get; }
        public float Width { get; }

        private int CenterIndex => VisibleZoneCount / 2;

        public ZoneProgressBarLayout(
            int visibleZoneCount,
            float cellWidth,
            float cellSpacing,
            float barHeight,
            float currentMarkerHeight)
        {
            if (visibleZoneCount < 1)
                throw new ArgumentOutOfRangeException(nameof(visibleZoneCount));
            if (cellWidth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cellWidth));
            if (cellSpacing < 0f)
                throw new ArgumentOutOfRangeException(nameof(cellSpacing));
            if (barHeight <= 0f)
                throw new ArgumentOutOfRangeException(nameof(barHeight));
            if (currentMarkerHeight <= 0f)
                throw new ArgumentOutOfRangeException(nameof(currentMarkerHeight));

            VisibleZoneCount = visibleZoneCount % 2 == 0
                ? visibleZoneCount + 1
                : visibleZoneCount;
            CellWidth = cellWidth;
            BarHeight = barHeight;
            CurrentMarkerHeight = currentMarkerHeight;
            CellStride = cellWidth + cellSpacing;
            Width = VisibleZoneCount * cellWidth +
                    (VisibleZoneCount - 1) * cellSpacing +
                    cellWidth * 0.5f;
        }

        public int FirstVisibleZone(int zone) => zone - CenterIndex;

        public float CellPosition(int index) => (index - CenterIndex) * CellStride;
    }
}
