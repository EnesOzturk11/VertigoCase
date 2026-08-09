using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    [Serializable]
    public sealed class ZoneProgressBarStyle
    {
        [SerializeField] private ZoneType type;
        [SerializeField] private Color futureTextColor = Color.white;
        [SerializeField] private Color pastTextColor = Color.gray;
        [SerializeField] private Color currentTextColor = Color.black;
        [SerializeField] private Color markerColor = Color.white;

        public ZoneType Type => type;
        public Color FutureTextColor => futureTextColor;
        public Color PastTextColor => pastTextColor;
        public Color CurrentTextColor => currentTextColor;
        public Color MarkerColor => markerColor;
    }

    /// <summary>
    /// Data-driven visual policy for the zone bar. Supporting a new ZoneType requires only a new
    /// style entry in this asset; progress-bar rendering code remains unchanged.
    /// </summary>
    [CreateAssetMenu(fileName = "ZoneProgressBarTheme", menuName = "Vertigo/UI/Zone Progress Bar Theme")]
    public sealed class ZoneProgressBarTheme : ScriptableObject
    {
        [Header("Zone styles")]
        [SerializeField] private List<ZoneProgressBarStyle> styles =
            new List<ZoneProgressBarStyle>();

        public ZoneProgressBarStyle StyleFor(ZoneType type)
        {
            for (int i = 0; i < styles.Count; i++)
            {
                ZoneProgressBarStyle style = styles[i];
                if (style != null && style.Type == type)
                    return style;
            }

            throw new KeyNotFoundException($"Zone progress bar theme has no style for {type}.");
        }

        public void ValidateConfiguration()
        {
            if (styles == null || styles.Count == 0)
                throw new InvalidOperationException("Zone progress bar theme requires zone styles.");

            var configuredTypes = new HashSet<ZoneType>();
            for (int i = 0; i < styles.Count; i++)
            {
                ZoneProgressBarStyle style = styles[i];
                if (style == null)
                    throw new InvalidOperationException($"Zone bar style at index {i} is missing.");
                if (!configuredTypes.Add(style.Type))
                    throw new InvalidOperationException(
                        $"Zone bar theme contains a duplicate style for {style.Type}.");
            }

            foreach (ZoneType type in Enum.GetValues(typeof(ZoneType)))
            {
                if (!configuredTypes.Contains(type))
                    throw new InvalidOperationException(
                        $"Zone bar theme is missing a style for {type}.");
            }
        }
    }
}
