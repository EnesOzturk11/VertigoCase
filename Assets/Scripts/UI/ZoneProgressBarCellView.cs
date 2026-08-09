using System;
using TMPro;
using UnityEngine;

namespace VertigoCase.UI
{
    /// <summary>Authored, reusable view for one zone number.</summary>
    public sealed class ZoneProgressBarCellView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        public RectTransform RectTransform => (RectTransform)transform;

        private void Awake()
        {
            if (label == null)
                throw new InvalidOperationException("Zone progress cell label is missing.");
        }

        public void Show(int zone, Color color)
        {
            gameObject.SetActive(true);
            label.text = zone.ToString();
            label.color = color;
        }

        public void Hide()
        {
            label.text = string.Empty;
            gameObject.SetActive(false);
        }

        public void SetColor(Color color) => label.color = color;
    }
}
