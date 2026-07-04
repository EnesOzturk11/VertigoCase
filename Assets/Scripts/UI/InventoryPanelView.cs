using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    /// <summary>
    /// The inventory panel: an Open button fills a scroll list with one row per collected reward type
    /// (icon + amount), and a Close button hides it. Reads GameController.Inventory and reacts to
    /// OnInventoryChanged. A thin View — no game logic. Like the GameOver panel, this component must sit
    /// on an ALWAYS-ACTIVE object; it toggles the child <see cref="panelRoot"/>.
    /// </summary>
    public class InventoryPanelView : MonoBehaviour
    {
        // Maps a reward type to the sprite shown for it. Filled in the Inspector.
        [Serializable]
        private class RewardIcon
        {
            public RewardType type;
            public Sprite icon;
        }

        [Header("References")]
        [SerializeField] private GameController game;
        [SerializeField] private GameObject panelRoot;        // the scroll panel shown/hidden
        [SerializeField] private Transform content;           // Scroll View -> Viewport -> Content
        [SerializeField] private InventoryRowView rowPrefab;  // one instance per reward type
        [SerializeField] private Button openButton;           // HUD button that opens the panel
        [SerializeField] private Button closeButton;          // button inside the panel

        [Header("Icons (RewardType -> Sprite)")]
        [SerializeField] private RewardIcon[] icons;

        private void OnEnable()
        {
            if (openButton != null)  openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (game != null)        game.OnInventoryChanged += HandleInventoryChanged;
            Close(); // start hidden
        }

        private void OnDisable()
        {
            if (openButton != null)  openButton.onClick.RemoveListener(Open);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (game != null)        game.OnInventoryChanged -= HandleInventoryChanged;
        }

        public void Open()
        {
            Rebuild();
            panelRoot.SetActive(true);
        }

        public void Close() => panelRoot.SetActive(false);

        // Keep the list fresh if a reward is won while the panel is open.
        private void HandleInventoryChanged(IReadOnlyDictionary<RewardType, int> inventory)
        {
            if (panelRoot.activeSelf) Rebuild();
        }

        // Wipe the old rows and spawn one per collected type, in the Inspector's icon order.
        private void Rebuild()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            foreach (RewardIcon entry in icons)
            {
                int amount = game.Inventory.TryGetValue(entry.type, out int value) ? value : 0;
                if (amount <= 0) continue; // skip types not collected yet

                InventoryRowView row = Instantiate(rowPrefab, content);
                row.Bind(entry.icon, amount);
            }
        }
    }
}