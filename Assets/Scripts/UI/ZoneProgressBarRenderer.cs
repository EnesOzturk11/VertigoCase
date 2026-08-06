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
                    elements.Labels[i].text = string.Empty;
                    elements.Labels[i].color = Color.clear;
                    elements.CellImages[i].color = Color.clear;
                    continue;
                }

                ZoneProgressBarStyle style = theme.StyleFor(typeOf(zone));
                elements.Labels[i].text = zone.ToString();
                elements.Labels[i].color = zone < currentZone
                    ? style.PastTextColor
                    : style.FutureTextColor;
                elements.CellImages[i].color = theme.CellTint;
            }

            ZoneProgressBarStyle currentStyle = theme.StyleFor(typeOf(currentZone));
            int currentIndex = currentZone - firstVisibleZone;
            if (currentIndex >= 0 && currentIndex < elements.Labels.Length)
                elements.Labels[currentIndex].color = currentStyle.CurrentTextColor;

            return currentStyle.MarkerColor;
        }
    }
}
