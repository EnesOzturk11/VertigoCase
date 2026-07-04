using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoCase.UI
{
    /// <summary>
    /// One row in the inventory list: a reward icon plus its collected amount. A dumb View — the
    /// panel spawns one per reward type and calls <see cref="Bind"/>. It holds no game logic.
    /// </summary>
    public class InventoryRowView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;            // ui_image_inventory_icon
        [SerializeField] private TextMeshProUGUI amountText; // ui_text_inventory_value

        public void Bind(Sprite icon, int amount)
        {
            if (icon != null) iconImage.sprite = icon;
            amountText.text = "x" + amount;
        }
    }
}