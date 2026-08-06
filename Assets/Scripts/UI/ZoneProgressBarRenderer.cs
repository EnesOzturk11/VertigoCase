using System;
using UnityEngine;
using VertigoCase.Config;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>Writes zone values and theme styles into an existing progress-bar hierarchy.</summary>
    internal sealed class ZoneProgressBarRenderer
    {
        private readonly ZoneProgressBarElements elements;
        private readonly ZoneProgressBarLayout layout;
        private readonly ZoneProgressBarTheme theme;

        public ZoneProgressBarRenderer(
            ZoneProgressBarElements elements,
            ZoneProgressBarLayout layout,
            ZoneProgressBarTheme theme)
        {
            this.elements = elements;
            this.layout = layout;
            this.theme = theme;
        }

        public Color Render(
            int currentZone,
            int firstVisibleZone,
            Func<int, ZoneType> typeOf)
        {
            for (int i = 0; i < layout.VisibleZoneCount; i++)
            {
                int zone = firstVisibleZone + i;

                if (zone < GameConstants.Zones.FirstZone)
                {
                    elements.Cells[i].Hide();
                    continue;
                }

                ZoneProgressBarStyle style = theme.StyleFor(typeOf(zone));
                elements.Cells[i].Show(
                    zone,
                    zone < currentZone ? style.PastTextColor : style.FutureTextColor);
            }

            ZoneProgressBarStyle currentStyle = theme.StyleFor(typeOf(currentZone));
            int currentIndex = currentZone - firstVisibleZone;
            if (currentIndex >= 0 && currentIndex < elements.Cells.Length)
                elements.Cells[currentIndex].SetColor(currentStyle.CurrentTextColor);

            return currentStyle.MarkerColor;
        }
    }
}
