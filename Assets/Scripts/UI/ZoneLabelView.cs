using UnityEngine;
using TMPro;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    /// <summary>
    /// Shows the current zone as a label like "Zone 5 (Safe)". A pure View: it subscribes to
    /// GameController's zone event and writes the text. No game logic, no polling.
    /// </summary>
    public class ZoneLabelView : MonoBehaviour
    {
        [SerializeField] private GameController game;
        [SerializeField] private TextMeshProUGUI zoneText;     // ui_text_zone_value

        private void OnEnable()  => game.OnZoneChanged += HandleZone;
        private void OnDisable() => game.OnZoneChanged -= HandleZone;

        private void HandleZone(int zone, ZoneType type) => zoneText.text = $"Zone {zone} ({type})";
    }
}