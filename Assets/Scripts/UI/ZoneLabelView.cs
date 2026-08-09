using System;
using UnityEngine;
using TMPro;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the current zone as a label like "Zone 5 (Safe)". A pure View: it subscribes through
    /// <see cref="IZonePort"/> and writes the text. No game logic or polling.
    /// </summary>
    public class ZoneLabelView : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour game;
        [SerializeField] private TextMeshProUGUI zoneText;     // ui_text_zone_value

        private IZonePort gamePort;

        private void Awake()
        {
            gamePort = game as IZonePort ??
                       throw new InvalidOperationException(
                           "ZoneLabelView requires a component implementing IZonePort.");
            if (zoneText == null)
                throw new InvalidOperationException("Zone label is missing.");
        }

        private void OnEnable()
        {
            gamePort.OnZoneChanged += HandleZone;
            HandleZone(gamePort.CurrentZone, gamePort.CurrentZoneType);
        }

        private void OnDisable()
        {
            if (gamePort != null)
                gamePort.OnZoneChanged -= HandleZone;
        }

        private void HandleZone(int zone, ZoneType type) => zoneText.text = $"Zone {zone} ({type})";
    }
}
